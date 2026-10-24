/*Copyright 2024 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using Audio;
using Quartz.Inputs;
using Quartz.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Quartz
{
    public class XUiC_MiniMap : XUiC_MinimapStats
    {
        private const string TAG = "XUiC_Minimap";

        public const int MapDrawnSize = 1024;
        public const int MapDrawnSizeInChunks = 64;
        public const int MapUpdateRadiusDefault = 128;

        private const float maxZoomScale = 5f;
        private const float minZoomScale = 0.5f;

        [XuiBindComponent("mapViewTexture", true)]
        private XUiView_WidgetBased xuiTexture;
        [XuiBindComponent("clippingPanel", true)]
        private XUiView clippingPanel;

        public const int ChunkDataLength = 256;

        public int bufferRowLength = MapDrawnSizeInChunks * ChunkDataLength;
        public int mapUpdateSizeRadius = 512;

        private int spriteSize = 50;
        private int spriteMax = 100;
        private int spriteMin = 10;

        private const float MapOnScreenSize = 300f;
        private const float MapDefaultZoom = 150f; //Vanilla 336

        private Texture2D mapTexture;

        private Vector2i cTexMiddle = new Vector2i(356, 356);

        private bool bMapInitialized;

        private bool bShouldRedrawMap;
        private float timeToRedrawMap;

        private Vector2 newMapMiddlePosChunks;
        private Vector2 mapMiddlePosChunks;
        private Vector2 mapMiddlePosPixel;
        private Vector2 mapMiddlePosChunksToServer;

        private Vector2 mapPos;

        private float mapScale = 1f;
        private float zoomScale;
        private float targetZoomScale;

        private Vector2 screenToMapScale;

        private float smoothTime = 0.3f;
        private float velocity;

        private int targetZoomIndex = 1;
        private float[] zoomSteps = new float[] {0.5f, 1f, 2f, 5f};

        private Transform transformSpritesParent;
        private Material mapMaterial;

        private HashSetLong navKeys = new HashSetLong();
        private DictionarySave<int, MinimapMarker> keyToNavSprite = new DictionarySave<int, MinimapMarker>();
        private HashSetLong navObjectsOnMapAlive = new HashSetLong();

        private ushort[] emptyChunk = new ushort[ChunkDataLength];
        private ushort[] mapColorsData;
        public NativeArray<ushort> rawBuffer;
        private NativeArray<Color32> colorLut;

        private JobHandle jobHandle;
        private bool jobDispatched = false;
        private bool isUpdating = false;
        private bool bResetMapData = false;
        private bool bFullTextureUpdate = false;

        private GameObject prefabMapSprite;

        private bool isOpen;

        private bool forceTextOff = false;

        [XuiXmlAttribute("iconsize")]
        public int SpriteSize { get => spriteSize; set => spriteSize = value; }

        [XuiXmlAttribute("iconscalemax")]
        public int SpriteMax { get => spriteMax; set => spriteMax = value; }

        [XuiXmlAttribute("iconscalemin")]
        public int SpriteMin { get => spriteMin; set => spriteMin = value; }

        [XuiXmlAttribute("forceicontextoff")]
        public bool ForceTextOff { get => forceTextOff; set => forceTextOff = value; }

        public override void Init()
        {
            base.Init();

            if(mapTexture == null)
            {
                mapTexture = new Texture2D(MapDrawnSize, MapDrawnSize, TextureFormat.RGBA32, false);
            }

            if (mapColorsData == null)
            {
                mapColorsData = new ushort[MapDrawnSize * MapDrawnSize];
            }

            if (!rawBuffer.IsCreated)
            {
                rawBuffer = new NativeArray<ushort>(MapDrawnSize * MapDrawnSize, Allocator.Persistent);
            }

            if (!colorLut.IsCreated)
            {
                colorLut = new NativeArray<Color32>(ushort.MaxValue + 1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < colorLut.Length; i++)
                {
                    colorLut[i] = MapJob.FromColor5To32((uint)i);
                }
            }

            xuiTexture.IsVisible = MinimapSettings.Enabled;

            clippingPanel.IsVisible = MinimapSettings.Enabled;
            transformSpritesParent = clippingPanel.UiTransform;

            zoomScale = mapScale;
            targetZoomScale = mapScale;

            UpdateMapUpdateRadius(zoomScale);

            xui.LoadData("Prefabs/MapSpriteEntity", delegate (GameObject o)
            {
                prefabMapSprite = o;
            });

            bShouldRedrawMap = true;
            InitMap();
            NavObjectManager.Instance.OnNavObjectRemoved += Instance_OnNavObjectRemoved;

            // The top bit marks a pixel as unexplored
            for (int i = 0; i < emptyChunk.Length; i++)
            {
                emptyChunk[i] = 1 << 15;
            }

            ResetMapColorsData();
        }

        private void Instance_OnNavObjectRemoved(NavObject newNavObject)
        {
            MinimapMarker gameObject = keyToNavSprite[newNavObject.Key];
            if(gameObject == null)
            {
                return;
            }

            gameObject.Clear();
            navKeys.Remove(newNavObject.Key);
            keyToNavSprite.Remove(newNavObject.Key);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            if (!isOpen)
            {
                isOpen = true;
                localPlayer = xui.playerUI.entityPlayer;

                QuartzInputManager.minimapActions.Enabled = true;

                InitMap();
                if (MinimapSettings.Enabled)
                {
                    UpdateFullMapNow();
                }
                xuiTexture.widget.onRender += OnRenderDrawcall;
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            if (isOpen)
            {
                isOpen = false;
                bShouldRedrawMap = false;

                QuartzInputManager.minimapActions.Enabled = false;
                xuiTexture.widget.onRender -= OnRenderDrawcall;
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (!windowGroup.isShowing || !XUi.IsGameRunning() || base.xui.playerUI.entityPlayer == null)
            {
                return;
            }

            if (!bMapInitialized)
            {
                InitMap();
            }

            bool allowMinimapActions = !xui.playerUI.windowManager.IsInputActive();

            if (QuartzInputManager.minimapActions.MinimapToggle.WasPressed && allowMinimapActions)
            {
                MinimapSettings.Enabled = !MinimapSettings.Enabled;
                bShouldRedrawMap |= true;
                xuiTexture.IsVisible = MinimapSettings.Enabled;
                clippingPanel.IsVisible = MinimapSettings.Enabled;
            }

            if (!MinimapSettings.Enabled)
            {
                return;
            }

            if(QuartzInputManager.minimapActions.MinimapZoomIn.WasPressed && allowMinimapActions)
            {
                MapZoomed(targetZoomIndex - 1);
            }

            if(QuartzInputManager.minimapActions.MinimapZoomOut.WasPressed && allowMinimapActions)
            {
                MapZoomed(targetZoomIndex + 1);
            }

            SetZoomLevel();

            if (jobDispatched && jobHandle.IsCompleted)
            {
                jobHandle.Complete();
                jobDispatched = false;
                mapTexture.Apply(false);
                mapMiddlePosChunks = newMapMiddlePosChunks;
                SendMapPositionToServer();
            }

            // Only one update may be in flight, as the coroutine writes the buffers the job reads
            if (bShouldRedrawMap && !isUpdating && !jobDispatched)
            {
                bShouldRedrawMap = false;
                ThreadManager.StartCoroutine(UpdateFullMapCoroutine(true));
            }

            if (timeToRedrawMap >= 0f)
            {
                timeToRedrawMap -= dt;
                if (timeToRedrawMap <= 0f)
                {
                    timeToRedrawMap = 2f;
                    bShouldRedrawMap = true;
                }
            }

            if (localPlayer.ChunkObserver.mapDatabase.IsNetworkDataAvail())
            {
                timeToRedrawMap = 0.5f;
                localPlayer.ChunkObserver.mapDatabase.ResetNetworkDataAvail();
            }

            PositionMapAtPlayer();
            UpdateMapObjects();
        }

        [XuiXmlAttribute("zoomscalesteps")]
        private void SetZoomscaleSteps(string value)
        {
            HashSet<float> zoomStepsSet = new HashSet<float>();
            string[] zoomStepsStrings = value.Split(',');

            for (int i = 0; i < zoomStepsStrings.Length; i++)
            {
                if (float.TryParse(zoomStepsStrings[i], out float result))
                {
                    if (minZoomScale <= result && result <= maxZoomScale)
                    {
                        zoomStepsSet.Add(result);
                    }
                }
            }

            zoomStepsSet.Add(1f);

            zoomSteps = zoomStepsSet.ToArray();
            Array.Sort(zoomSteps);

            for (int i = 0; i < zoomSteps.Length; i++)
            {
                if (zoomSteps[i] == 1f)
                {
                    targetZoomIndex = i;
                    break;
                }
            }
        }

        private void InitMap()
        {
            if (xui.playerUI.entityPlayer != null)
            {
                localPlayer = xui.playerUI.entityPlayer;
                bMapInitialized = true;
                if(xuiTexture is XUiV_Texture texture)
                {
                    mapMaterial = texture.material;
                    texture.Texture = mapTexture;
                }

                if (xuiTexture is XUiV_MaskedTexture maskedTexture)
                {
                    mapMaterial = maskedTexture.Material;
                    maskedTexture.Texture = mapTexture;
                }

                if (mapMaterial != null)
                {
                    mapMaterial.SetTexture("_MainTex", mapTexture);
                    mapMaterial.shader = LoadMinimapShader();
                }

                cTexMiddle = xuiTexture.Size / 2;

                screenToMapScale = new Vector2(xuiTexture.Size.x / MapDefaultZoom / zoomScale, xuiTexture.Size.y / MapDefaultZoom / zoomScale);

                xuiTexture.IsVisible = MinimapSettings.Enabled;
                clippingPanel.IsVisible = MinimapSettings.Enabled;
            }
        }

        private void UpdateFullMapNow()
        {
            if (isUpdating || jobDispatched)
            {
                bShouldRedrawMap = true;
                return;
            }

            IEnumerator update = UpdateFullMapCoroutine(false);
            while (update.MoveNext()) { }
        }

        private IEnumerator UpdateFullMapCoroutine(bool spreadOverFrames)
        {
            isUpdating = true;

            if (bResetMapData)
            {
                bResetMapData = false;
                ResetMapColorsData();
            }

            Vector3 worldPos = localPlayer.GetPosition();
            int worldPosX = (int)worldPos.x;
            int worldPosY = (int)worldPos.z;
            Vector2 middlePosChunk = new Vector2(World.toChunkXZ(worldPosX - 1024) * 16 + 1024, World.toChunkXZ(worldPosY - 1024) * 16 + 1024);

            newMapMiddlePosChunks = middlePosChunk;

            int worldChunkStartX = World.toChunkXZ((int)newMapMiddlePosChunks.x - mapUpdateSizeRadius);
            int worldChunkStartY = World.toChunkXZ((int)newMapMiddlePosChunks.y - mapUpdateSizeRadius);
            int drawnChunkStart = ((MapDrawnSize / 2) - mapUpdateSizeRadius) / 16;
            int chunkCount = Math.Min((2 * mapUpdateSizeRadius + 15) / 16, MapDrawnSizeInChunks - drawnChunkStart);

            int dirtyMinX = int.MaxValue;
            int dirtyMinY = int.MaxValue;
            int dirtyMaxX = -1;
            int dirtyMaxY = -1;

            IMapChunkDatabase mapDatabase = localPlayer.ChunkObserver.mapDatabase;
            for (int y = 0; y < chunkCount; y++)
            {
                int drawnChunkY = drawnChunkStart + y;
                for (int x = 0; x < chunkCount; x++)
                {
                    int drawnChunkX = drawnChunkStart + x;
                    int chunkX = worldChunkStartX + x;
                    int chunkZ = worldChunkStartY + y;
                    ushort[] mapColors = mapDatabase.GetMapColors(WorldChunkCache.MakeChunkKey(chunkX, chunkZ));
                    if (mapColors == null)
                    {
                        mapColors = emptyChunk;
                    }
                    else if (localPlayer.world.GetChunkSync(chunkX, chunkZ) is Chunk chunk && chunk.bMapDirty && !chunk.NeedsDecoration)
                    {
                        // Vanilla only refreshes the map database when the player changes chunk,
                        // so push block changes from loaded, already explored chunks here.
                        // The database copies into its existing array, so mapColors stays current.
                        mapDatabase.Add(chunkX, chunkZ, chunk.GetMapColors());
                    }

                    int indexBuffer = (drawnChunkY * bufferRowLength) + (drawnChunkX * ChunkDataLength);
                    if (ChunkEquals(mapColors, mapColorsData, indexBuffer))
                    {
                        continue;
                    }

                    Array.Copy(mapColors, 0, mapColorsData, indexBuffer, ChunkDataLength);
                    NativeArray<ushort>.Copy(mapColors, 0, rawBuffer, indexBuffer, ChunkDataLength);

                    dirtyMinX = Math.Min(dirtyMinX, drawnChunkX);
                    dirtyMaxX = Math.Max(dirtyMaxX, drawnChunkX);
                    dirtyMinY = Math.Min(dirtyMinY, drawnChunkY);
                    dirtyMaxY = Math.Max(dirtyMaxY, drawnChunkY);
                }

                if (spreadOverFrames)
                {
                    yield return null;

                    if (!rawBuffer.IsCreated)
                    {
                        isUpdating = false;
                        yield break;
                    }
                }
            }

            if (bFullTextureUpdate)
            {
                bFullTextureUpdate = false;
                dirtyMinX = 0;
                dirtyMinY = 0;
                dirtyMaxX = MapDrawnSizeInChunks - 1;
                dirtyMaxY = MapDrawnSizeInChunks - 1;
            }

            if (dirtyMaxX < 0)
            {
                // Nothing changed, so skip the texture conversion and upload
                mapMiddlePosChunks = newMapMiddlePosChunks;
                SendMapPositionToServer();
                isUpdating = false;
                yield break;
            }

            MapJob job = new MapJob
            {
                data = rawBuffer,
                lut = colorLut,
                texture = mapTexture.GetRawTextureData<Color32>(),
                startChunkX = dirtyMinX,
                endChunkX = dirtyMaxX + 1,
                startChunkY = dirtyMinY
            };

            jobHandle = job.ScheduleParallelByRef(dirtyMaxY - dirtyMinY + 1, 1, default);
            JobHandle.ScheduleBatchedJobs();
            jobDispatched = true;
            isUpdating = false;
        }

        private static bool ChunkEquals(ushort[] chunk, ushort[] data, int dataIndex)
        {
            for (int i = 0; i < ChunkDataLength; i++)
            {
                if (chunk[i] != data[dataIndex + i])
                {
                    return false;
                }
            }

            return true;
        }

        private void SendMapPositionToServer()
        {
            if (GameManager.Instance.World.IsRemote() && !mapMiddlePosChunksToServer.Equals(mapMiddlePosChunks))
            {
                mapMiddlePosChunksToServer = mapMiddlePosChunks;
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageMapPosition>().Setup(localPlayer.entityId, new Vector2i(global::Utils.Fastfloor(mapMiddlePosChunks.x), global::Utils.Fastfloor(mapMiddlePosChunks.y))));
            }
        }

        private void PositionMap()
        {
            float yScale = xuiTexture.Size.y / (float) xuiTexture.Size.x;

            //float numX = (MapDrawnSize - (MapDefaultZoom * xScale) * zoomScale) / 2f;
            float numX = (MapDrawnSize - MapDefaultZoom * zoomScale) / 2f;
            float numY = (MapDrawnSize - (MapDefaultZoom * yScale) * zoomScale) / 2f;

            mapScale = MapDefaultZoom * zoomScale / MapDrawnSize;
            float x = (numX + (mapMiddlePosPixel.x - mapMiddlePosChunks.x)) / MapDrawnSize;
            float y = (numY + (mapMiddlePosPixel.y - mapMiddlePosChunks.y)) / MapDrawnSize;
            mapPos = new Vector3(x, y, 0f);
        }

        private void OnRenderDrawcall(Material mat)
        {
            float yScale = xuiTexture.Size.y / (float)xuiTexture.Size.x;
            float rotation = MinimapSettings.FollowPlayerView ? -localPlayer.rotation.y * Mathf.Deg2Rad : 0f;

            mat.SetVector("_MainMapPosAndScale", new Vector4(mapPos.x, mapPos.y, mapScale, mapScale * yScale));
            mat.SetFloat("_MapRotation", rotation);
            mat.SetFloat("_MapOpacity", MinimapSettings.TextureOpacity);

        }

        private void MapZoomed(int newZoomIndex)
        {
            if(newZoomIndex < 0 || newZoomIndex >= zoomSteps.Length)
            {
                return;
            }

            bool zoomingOut = targetZoomIndex < newZoomIndex;

            targetZoomIndex = global::Utils.FastClamp(newZoomIndex, 0, zoomSteps.Length - 1);
            targetZoomScale = zoomSteps[targetZoomIndex];

            if(zoomingOut)
            {
                UpdateMapUpdateRadius(targetZoomScale);
                Manager.PlayInsidePlayerHead("map_zoom_out");
            } 
            else
            {
                Manager.PlayInsidePlayerHead("map_zoom_in");
            }
        }

        private void SetZoomLevel()
        {
            if(zoomScale == targetZoomScale)
            {
                return;
            }

            zoomScale = Mathf.SmoothDamp(zoomScale, targetZoomScale, ref velocity, smoothTime);
            float diff = Math.Abs(targetZoomScale - zoomScale);
            if ((diff / targetZoomScale) < 0.005)
            {
                zoomScale = targetZoomScale;
                velocity = 0;

                // Deferred to the next update, as the buffers can't be touched while an update is in flight
                bResetMapData = true;
                bShouldRedrawMap = true;
                UpdateMapUpdateRadius(zoomScale);
            }
            screenToMapScale = new Vector2(xuiTexture.Size.x / MapDefaultZoom / zoomScale, xuiTexture.Size.y / MapDefaultZoom / zoomScale);
        }

        private void UpdateMapUpdateRadius(float zoomScale)
        {
            if(zoomScale < 1f)
            {
                zoomScale = 1f;
            }

            int newMapUpdateSizeRadius = (int)(MapUpdateRadiusDefault * zoomScale);
            if (newMapUpdateSizeRadius > (MapDrawnSize / 2))
            {
                newMapUpdateSizeRadius = MapDrawnSize / 2;
            }

            bShouldRedrawMap |= newMapUpdateSizeRadius != mapUpdateSizeRadius;
            mapUpdateSizeRadius = newMapUpdateSizeRadius;
        }

        private float GetSpriteZoomScaleFactor()
        {
            return global::Utils.FastClamp(1f / (zoomScale * 2f), 0.02f, 20f);
        }

        private void UpdateNavObjectList()
        {
            List<NavObject> navObjectList = NavObjectManager.Instance.NavObjectList;
            navObjectsOnMapAlive.Clear();

            Vector3 middlePos = transformSpritesParent.TransformPoint(WorldPosToScreenPos(localPlayer.position));
            Color opacity = new Color(1f, 1f, 1f, MinimapSettings.IconOpacity);
            float spriteScale = spriteSize * MinimapSettings.IconScaleModifer;
            float spriteZoomScale = GetSpriteZoomScaleFactor();

            for (int i = 0; i < navObjectList.Count; i++)
            {
                NavObject navObject = navObjectList[i];
                int key = navObject.Key;
                NavObjectMapSettings currentMapSettings = navObject.CurrentMapSettings;
                if (currentMapSettings != null && navObject.IsOnMiniMap())
                {
                    MinimapMarker mapObject;
                    if (!keyToNavSprite.ContainsKey(key))
                    {
                        mapObject = new MinimapMarker(transformSpritesParent.gameObject.AddChild(prefabMapSprite));
                        string spriteName = navObject.GetSpriteName(currentMapSettings);
                        mapObject.sprite.atlas = xui.GetAtlasByName(((UnityEngine.Object)mapObject.sprite.atlas).name, spriteName);
                        mapObject.sprite.spriteName = spriteName;
                        mapObject.sprite.depth = currentMapSettings.Layer;
                        mapObject.label.font = xui.GetUIFontByName("ReferenceFont");
                        navKeys.Add(key);
                        keyToNavSprite[key] = mapObject;
                    }
                    else
                    {
                        mapObject = keyToNavSprite[key];
                    }

                    string displayName = navObject.DisplayName;
                    if (!forceTextOff && !string.IsNullOrEmpty(displayName) && MinimapSettings.ShowText)
                    {
                        mapObject.label.text = displayName;
                        mapObject.label.gameObject.SetActive(true);
                        mapObject.label.color = (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color) * opacity;
                    }
                    else
                    {
                        mapObject.label.text = "";
                        mapObject.label.gameObject.SetActive(false);
                    }

                    Vector3 vector = currentMapSettings.IconScaleVector * spriteZoomScale;
                    mapObject.sprite.width = Mathf.Clamp((int)(spriteScale * vector.x), spriteMin, spriteMax);
                    mapObject.sprite.height = Mathf.Clamp((int)(spriteScale * vector.y), spriteMin, spriteMax);
                    mapObject.sprite.color = (navObject.hiddenOnCompass ? Color.grey : (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color)) * opacity;

                    //if (MinimapSettings.FollowPlayerView && navObject.TrackedEntity == localPlayer)
                    //{
                    //    mapObject.spriteTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    //}
                    if (MinimapSettings.FollowPlayerView && navObject.NavObjectClass.RequirementType == NavObjectClass.RequirementTypes.IsPlayer)
                    {
                        mapObject.spriteTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    }
                    else
                    {
                        mapObject.spriteTransform.localEulerAngles = new Vector3(0f, 0f, -navObject.Rotation.y);
                    }

                    mapObject.transform.localPosition = WorldPosToScreenPos(navObject.GetPosition() + Origin.position);

                    if (currentMapSettings.AdjustCenter)
                    {
                        mapObject.spriteTransform.localPosition += new Vector3(mapObject.sprite.width / 2, mapObject.sprite.height / 2, 0f);
                    }

                    //Rotates the mapObject around the player's position in the minimap
                    if (MinimapSettings.FollowPlayerView)
                    {
                        mapObject.transform.localEulerAngles = new Vector3(0f, 0f, -localPlayer.rotation.y);
                        mapObject.transform.RotateAround(middlePos, new Vector3(0, 0, 1), localPlayer.rotation.y);
                    }

                    navObjectsOnMapAlive.Add(key);
                }
            }
        }

        protected virtual void UpdateMapObjects()
        {
            World world = GameManager.Instance.World;
            navObjectsOnMapAlive.Clear();

            if(MinimapSettings.ShowIcons)
            {
                UpdateNavObjectList();
            }

            navKeys.RemoveWhere(removeNavSprite);

            localPlayer.selectedSpawnPointKey = -1L;
        }

        private bool removeNavSprite(long key)
        {
            if(!navObjectsOnMapAlive.Contains(key))
            {
                keyToNavSprite[(int)key].Clear();
                keyToNavSprite.Remove((int)key);
                return true;
            }

            return false;
        }

        private Vector3 WorldPosToScreenPos(Vector3 _worldPos)
        {
            return new Vector3((_worldPos.x - mapMiddlePosPixel.x) * screenToMapScale.x + (float)cTexMiddle.x, (_worldPos.z - mapMiddlePosPixel.y) * screenToMapScale.y - (float)cTexMiddle.y, 0f);
        }

        public void PositionMapAtPlayer()
        {
            Vector3 worldPos = localPlayer.GetPosition();
            mapMiddlePosPixel.x = worldPos.x;
            mapMiddlePosPixel.y = worldPos.z;

            PositionMap();
        }

        private void ResetMapColorsData()
        {
            for (int i = 0; i < mapColorsData.Length; i += ChunkDataLength)
            {
                Array.Copy(emptyChunk, 0, mapColorsData, i, ChunkDataLength);
            }

            rawBuffer.CopyFrom(mapColorsData);
            bFullTextureUpdate = true;
        }

        public override void Cleanup()
        {
            base.Cleanup();

            if (jobDispatched)
            {
                jobHandle.Complete();
                jobDispatched = false;
            }

            if (rawBuffer.IsCreated)
            {
                rawBuffer.Dispose();
            }

            if (colorLut.IsCreated)
            {
                colorLut.Dispose();
            }        }

        private Shader LoadMinimapShader()
        {
            return DataLoader.LoadAsset<Shader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/MaskedMinimap.shader");
        }

        private class MinimapMarker
        {
            public GameObject gameObject;
            public Transform transform;

            public UISprite sprite;
            public Transform spriteTransform;

            public UILabel label;

            public MinimapMarker(GameObject gameObject)
            {
                this.gameObject = gameObject;
                transform = gameObject.transform;

                sprite = transform.Find("Sprite").GetComponent<UISprite>();
                spriteTransform = sprite.transform;

                label = transform.Find("Name").GetComponent<UILabel>();
            }

            public void Clear()
            {
                UnityEngine.Object.Destroy(gameObject);
                transform = null;
                sprite = null;
                spriteTransform = null;
                label = null;
            }
        }

        public struct MapJob : IJobFor
        {
            [ReadOnly]
            public NativeArray<ushort> data;

            [ReadOnly]
            public NativeArray<Color32> lut;

            [NativeDisableParallelForRestriction]
            public NativeArray<Color32> texture;

            public int startChunkX;
            public int endChunkX;
            public int startChunkY;

            // Each index is one row of chunks, so reads and writes stay contiguous
            public void Execute(int index)
            {
                int chunkY = startChunkY + index;
                int rowPixel = chunkY * 16 * MapDrawnSize;

                for (int chunkX = startChunkX; chunkX < endChunkX; chunkX++)
                {
                    int dataIndex = ((chunkY * MapDrawnSizeInChunks) + chunkX) * ChunkDataLength;
                    int pixel = rowPixel + (chunkX * 16);
                    for (int py = 0; py < 16; py++, pixel += MapDrawnSize)
                    {
                        for (int px = 0; px < 16; px++)
                        {
                            texture[pixel + px] = lut[data[dataIndex++]];
                        }
                    }
                }
            }

            public static Color32 FromColor5To32(uint col)
            {
                if (col >> 15 == 0)
                {
                    byte r = (byte)((float)((col >> 10) & 31) / 31f * 255f);
                    byte g = (byte)((float)((col >> 5) & 31) / 31f * 255f);
                    byte b = (byte)((float)(col & 31) / 31f * 255f);
                    return new Color32(r, g, b, byte.MaxValue);
                }

                return new Color32(255, 255, 255, 0);
            }
        }
    }
}

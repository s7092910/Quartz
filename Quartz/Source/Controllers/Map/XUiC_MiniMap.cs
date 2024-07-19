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
using Quartz.Map;
using Quartz.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public int bufferRowLength = MapDrawnSizeInChunks * 128;
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
        private long maxMapTickTime = 0;

        private Vector2 mapMiddlePosChunks;
        private Vector2 mapMiddlePosPixel;
        private Vector2 mapMiddlePosChunksToServer;

        private Vector2 mapPos;

        private float mapScale = 1f;
        private float zoomScale;
        private float targetZoomScale;

        private float smoothTime = 0.3f;
        private float velocity;

        private int targetZoomIndex = 1;
        private float[] zoomSteps = new float[] {0.5f, 1f, 2f, 5f};

        private XUiView xuiTexture;
        private XUiView clippingPanel;
        private Transform transformSpritesParent;

        private HashSetLong navKeys = new HashSetLong();
        private DictionarySave<int, MinimapMarker> keyToNavSprite = new DictionarySave<int, MinimapMarker>();
        private HashSetLong navObjectsOnMapAlive = new HashSetLong();

        private uint[] emptyChunk = new uint[128];
        private ushort[] mapColorsShort;

        private int kernelIndex;

        private GameObject prefabMapSprite;

        private bool isOpen;

        private bool forceTextOff = false;

        public override void Init()
        {
            base.Init();

            //var maxSizeMb = SystemInfo.maxGraphicsBufferSize / 1024 / 1024;
            //Logging.Inform($"Maximum graphics buffer size is {maxSizeMb} MB");
            
            if (mapTexture == null)
            {
                mapTexture = new Texture2D(MapDrawnSize, MapDrawnSize, TextureFormat.RGB565, false);
                mapTexture.name = "Minimap";
                mapTexture.wrapMode = TextureWrapMode.Clamp;
                mapTexture.Apply(false, false);
                // mapTexture.enableRandomWrite = true;
                // mapTexture.Create();
            }

            if (mapColorsShort == null)
            {
                mapColorsShort = new ushort[MapDrawnSize * MapDrawnSize];
            }

            XUiController childById = GetChildById("mapViewTexture");
            if (childById != null)
            {
                xuiTexture = childById.ViewComponent;
                xuiTexture.IsVisible = MinimapSettings.Enabled;
            }

            childById = GetChildById("clippingPanel");
            if (childById != null)
            {
                clippingPanel = childById.ViewComponent;
                clippingPanel.IsVisible = MinimapSettings.Enabled;
                transformSpritesParent = clippingPanel.UiTransform;
            }

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

            ushort a = 1 << 15;
            uint emptyBlocks = MapColorDatabase.PackShorts(a, a);
            for (int i = 0; i < emptyChunk.Length; i++)
            {
                emptyChunk[i] = emptyBlocks;
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
                UpdateFullMap();
                LocalPlayerCamera localPlayerCamera = xui.playerUI.GetComponentInParent<LocalPlayerCamera>();

                if (localPlayerCamera != null)
                {
                    localPlayerCamera.PreRender += OnPreRender;
                }
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

                LocalPlayerCamera localPlayerCamera = xui.playerUI.GetComponentInParent<LocalPlayerCamera>();
                
                if(localPlayerCamera != null)
                {
                    localPlayerCamera.PreRender -= OnPreRender;
                }
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

            if (bShouldRedrawMap)
            {
                UpdateFullMap();
                bShouldRedrawMap = false;
            }

            MicroStopwatch stopwatch = new MicroStopwatch();
            stopwatch.Start();
            TickMapUpdate();
            stopwatch.Stop();

            if (maxMapTickTime < stopwatch.ElapsedMicroseconds)
            {
                maxMapTickTime = stopwatch.ElapsedMicroseconds;
                Log.Out("Got a new max map tick time => {0}",
                    stopwatch.ElapsedMicroseconds * 0.001d);
            }

            // if (timeToRedrawMap >= 0f)
            // {
            //     timeToRedrawMap -= dt;
            //     if (timeToRedrawMap <= 0f)
            //     {
            //         timeToRedrawMap = 2f;
            //         // bShouldRedrawMap = true;
            //     }
            // }

            if (localPlayer.ChunkObserver.mapDatabase.IsNetworkDataAvail())
            {
                timeToRedrawMap = 0.5f;
                localPlayer.ChunkObserver.mapDatabase.ResetNetworkDataAvail();
            }

            PositionMapAtPlayer();
            UpdateMapObjects();
        }

        public override bool ParseAttribute(string attribute, string value, XUiController parent)
        {
            switch (attribute)
            {
                case "zoomscalesteps":
                    HashSet<float> zoomStepsSet = new HashSet<float>();
                    string[] zoomStepsStrings = value.Split(',');

                    for (int i = 0; i < zoomStepsStrings.Length; i++)
                    {
                        if (float.TryParse(zoomStepsStrings[i], out float result))
                        {
                            if(minZoomScale <= result && result <= maxZoomScale)
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

                    return true;
                case "iconscalemax":
                    if(int.TryParse(value, out int max)) {
                        spriteMax = max;
                    }
                    return true;
                case "iconscalemin":
                    if (int.TryParse(value, out int min))
                    {
                        spriteMin = min;
                    }
                    return true;
                case "iconsize":
                    if (int.TryParse(value, out int scale))
                    {
                        spriteSize = scale;
                    }
                    return true;
                case "forceicontextoff":
                    if (bool.TryParse(value, out bool force))
                    {
                        forceTextOff = force;
                    }
                    return true;
                default:
                    return base.ParseAttribute(attribute, value, parent);
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
                    texture.Material.SetTexture("_MainTex", mapTexture);
                    texture.Material.shader = LoadMinimapShader();
                }

                if (xuiTexture is XUiV_MaskedTexture maskedTexture)
                {
                    maskedTexture.Material.SetTexture("_MainTex", mapTexture);
                    maskedTexture.Material.shader = LoadMinimapShader();
                }

                cTexMiddle = xuiTexture.Size / 2;

                xuiTexture.IsVisible = MinimapSettings.Enabled;
                clippingPanel.IsVisible = MinimapSettings.Enabled;
            }
        }

        bool tickRunning = false;
        bool tickFinished = false;

        int tickStateX = -1;
        int tickStateZ = -1;
        int tickChunkX = -1;
        int tickChunkZ = -1;
        int tickChunkStart = 0;
        int tickChunkEnd = 64;
        int tickMapStartX = 0;
        int tickMapStartZ = 0;

        private void TickMapUpdate()
        {
            Vector3 worldPos = localPlayer.GetPosition();
            mapMiddlePosChunks = new Vector2(
                World.toChunkXZ((int)worldPos.x - 1024) * 16 + 1024,
                World.toChunkXZ((int)worldPos.z - 1024) * 16 + 1024);

            if (tickRunning == false)
            {
                tickMapStartX = (int)mapMiddlePosChunks.x - 512;
                tickMapStartZ = (int)mapMiddlePosChunks.y - 512;
                tickChunkX = World.toChunkXZ(tickMapStartX);
                tickChunkZ = World.toChunkXZ(tickMapStartZ);
                tickChunkStart = 32 - mapUpdateSizeRadius / 16;
                tickChunkEnd = 32 + mapUpdateSizeRadius / 16;
                if (tickChunkStart < 0) tickChunkStart = 0;
                if (tickChunkEnd > 64) tickChunkEnd = 64;
                tickStateX = tickChunkStart;
                tickStateZ = tickChunkStart;
                tickRunning = true;
                mapMiddlePosPixel.x = worldPos.x;
                mapMiddlePosPixel.y = worldPos.z;
                PositionMap();

            }
            else if (tickFinished)
            {
                UpdateMapTextureFromRawArray();
                tickFinished = false;
                tickRunning = false;
            }
            else
            {
                IMapChunkDatabase mapDatabase = localPlayer.ChunkObserver.mapDatabase;

                // If called every frame (quota 42) => around one full updates per second with 100fps
                // For a full map update we need to update 4096 chunks => 100fps, 40 per frame
                // Cool thing is this will scale automatically down if fps is low already ;)
                // Note: could only use up quota if redraw does actually something!?
                int budget = 42;

                for (; tickStateZ < tickChunkEnd; tickStateZ++)
                {
                    for (; tickStateX < tickChunkEnd; tickStateX++)
                    {
                        if (budget-- <= 0) return;
                        RedrawChunkIntoRawArray(mapDatabase,
                            tickChunkX, tickStateX, tickChunkZ, tickStateZ);
                    }
                    tickStateX = tickChunkStart;
                }
                tickFinished = true;
            }
        }


        private void UpdateMapSectionCompute()
        {

            // mapUpdateSizeRadius = 512;

            // The full potential area of chunks to render
            int mapStartX = (int)mapMiddlePosChunks.x - 512;
            int mapStartZ = (int)mapMiddlePosChunks.y - 512;
            // int mapEndX = (int)mapMiddlePosChunks.x + 512;
            // int mapEndZ = (int)mapMiddlePosChunks.y + 512;

            int chunkStart = 32 - mapUpdateSizeRadius / 16;
            int chunkEnd = 32 + mapUpdateSizeRadius / 16;

            IMapChunkDatabase mapDatabase = localPlayer.ChunkObserver.mapDatabase;

            int sx = World.toChunkXZ(mapStartX);
            int sz = World.toChunkXZ(mapStartZ);

            for (int nz = chunkStart; nz < chunkEnd; nz++)
            {
                for (int nx = chunkStart; nx < chunkEnd; nx++)
                {
                    RedrawChunkIntoRawArray(mapDatabase, sx, nx, sz, nz);
                }
            }

            UpdateMapTextureFromRawArray();
        }


        private void UpdateFullMap()
        {
            Vector3 worldPos = localPlayer.GetPosition();
            int worldPosX = (int)worldPos.x;
            int worldPosY = (int)worldPos.z;

            Vector2 middlePosChunk = new Vector2(World.toChunkXZ(worldPosX - 1024) * 16 + 1024, World.toChunkXZ(worldPosY - 1024) * 16 + 1024);

            //if(mapMiddlePosChunks.Equals(middlePosChunk))
            //{
            //    return;
            //}

            mapMiddlePosChunks = middlePosChunk;

            MicroStopwatch stopwatch = new MicroStopwatch();
            stopwatch.Start();
            UpdateMapSectionCompute();
            stopwatch.Stop();
            Log.Out("updateFullMap Called, time taken = " + (stopwatch.ElapsedMicroseconds * 0.001d));

            PositionMapAtPlayer();
            SendMapPositionToServer();
        }


        public ushort SwapBytes(ushort x)
        {
            return (ushort)((ushort)((x & 0xff) << 8) | ((x >> 8) & 0xff));
        }
        
        public ushort FixColor(ushort x)
        {
            return (ushort)(
                // Blue channel is fine and so is lower red
                (x & 0b0000_0000_0001_1111) |
                ((x & 0b0111_1111_1110_0000) << 1)
            );
        }

        private void RedrawChunkIntoRawArray(IMapChunkDatabase mapDatabase, int sx, int nx, int sz, int nz)
        {
            long key = WorldChunkCache.MakeChunkKey(sx + nx, sz + nz);
            ushort[] mapColors = mapDatabase.GetMapColors(key);
            if (mapColors != null)
            {
                for (int u = 0; u < 16; u++)
                {
                    // Copy the full line at once (not sure this is faster than a simple loop though)
                    //Buffer.BlockCopy(mapColors, u * 16 * 2, mapColorsShort, (nz * 16 + u) * 1024 * 2 + nx * 16 * 2, 32);
                    for (int v = 0; v < 16; v++)
                    {
                        mapColorsShort[(nz * 16 + u) * 1024 + nx * 16 + v]
                              = FixColor(mapColors[u * 16 + v]);
                    //         = mapColors[u * 16 + v]; // Isn't RGB565!?
                    //         // = SwapBytes(mapColors[u * 16 + v]);
                    //         // = 0b1111_1000_0000_0000;
                    //         // = 0b0000_0111_1110_0000;
                    //         // = mapColors[u * 16 + v];
                    }
                }
            }
            else
            {
                // for (int u = 0; u < 16; u++)
                // {
                //     for (int v = 0; v < 16; v++)
                //     {
                //         // Log.Out("==> {0}", (nz * 16 + u) * 16 + (nx * 16 + v));
                //         //mapColorsShort[(nz * 16 + u) * 1024 + nx * 16 + v]
                //         //    = ushort.MaxValue / 32;
                //         // = mapColors[u * 16 + v];
                //     }
                // }
            }

        }

        private void UpdateMapTextureFromRawArray()
        {
            mapTexture.SetPixelData(mapColorsShort, 0, 0);
            mapTexture.Apply(false, false);
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

        private void OnPreRender(LocalPlayerCamera _localPlayerCamera)
        {

            float yScale = xuiTexture.Size.y / (float)xuiTexture.Size.x;
            float rotation = MinimapSettings.FollowPlayerView ? -localPlayer.rotation.y * Mathf.Deg2Rad : 0f;

            UIDrawCall drawCall = null;
            if (xuiTexture is XUiV_Texture texture)
            {
                drawCall = texture.UITexture.drawCall;
            }

            if (xuiTexture is XUiV_MaskedTexture maskedTexture)
            {
                drawCall = maskedTexture.UITexture.drawCall;
            }

            if (drawCall != null)
            {
                drawCall.dynamicMaterial.SetVector("_MainMapPosAndScale", new Vector4(mapPos.x, mapPos.y, mapScale, mapScale * yScale));
                drawCall.dynamicMaterial.SetFloat("_MapRotation", rotation);
                drawCall.dynamicMaterial.SetFloat("_MapOpacity", MinimapSettings.TextureOpacity);
            }

            //Shader.SetGlobalVector("_MainMapPosAndScale", new Vector4(mapPos.x, mapPos.y, mapScale, mapScale * yScale));
            //Shader.SetGlobalFloat("_MapRotation", rotation);
            //Shader.SetGlobalFloat("_MapOpacity", MinimapSettings.TextureOpacity);

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

                ResetMapColorsData();
                UpdateMapUpdateRadius(zoomScale);
            }
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

            bShouldRedrawMap = newMapUpdateSizeRadius != mapUpdateSizeRadius;
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
                if (navObject.HasRequirements && navObject.NavObjectClass.IsOnMiniMap(navObject.IsActive))
                {
                    NavObjectMapSettings currentMapSettings = navObject.CurrentMapSettings;
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
                        mapObject.spriteTransform.localEulerAngles = new Vector3(0f, 0f, 0f - navObject.Rotation.y);
                    }

                    if (currentMapSettings.AdjustCenter)
                    {
                        mapObject.spriteTransform.localPosition += new Vector3(mapObject.sprite.width / 2, mapObject.sprite.height / 2, 0f);
                    }
                    mapObject.transform.localPosition = WorldPosToScreenPos(navObject.GetPosition() + Origin.position);

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
            return new Vector3((_worldPos.x - mapMiddlePosPixel.x) * 2.11904764f / zoomScale + (float)cTexMiddle.x, (_worldPos.z - mapMiddlePosPixel.y) * 2.11904764f / zoomScale - (float)cTexMiddle.y, 0f);
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
            // for (int i = 0; i < mapColorsData.Length; i += 128)
            // {
            //     Array.Copy(emptyChunk, 0, mapColorsData, i, 128);
            // }
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private ComputeShader LoadComputeShader()
        {
            return DataLoader.LoadAsset<ComputeShader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/MinimapCreation.compute");
        }

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
    }
}

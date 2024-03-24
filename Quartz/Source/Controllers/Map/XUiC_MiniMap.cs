using Audio;
using Quartz.Inputs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using XMLData.Parsers;

namespace Quartz
{
    public class XUiC_MiniMap : XUiController
    {
        private const string TAG = "XUiC_Minimap";

        public const int MapDrawnSize = 1024;
        public const int MapDrawnSizeInChunks = 64;
        public const int MapUpdateRadiusDefault = 128;

        private const float maxZoomScale = 5f;
        private const float minZoomScale = 0.5f;

        public int bufferRowLength = MapDrawnSizeInChunks * 128;
        public int mapUpdateSizeRadius = 512;

        private int cSpriteScale = 50;

        private const float MapOnScreenSize = 300f;
        private const float MapDefaultZoom = 150f; //Vanilla 336

        private RenderTexture mapTextureRender;

        private Vector2i cTexMiddle = new Vector2i(356, 356);

        private bool bMapInitialized;

        private bool bShouldRedrawMap;
        private float timeToRedrawMap;

        private Vector2 mapMiddlePosChunks;
        private Vector2 mapMiddlePosPixel;
        private Vector2 mapMiddlePosChunksToServer;

        private Vector2 mapPos;

        private float zoomScale;
        private float targetZoomScale;

        private float smoothTime = 0.3f;
        private float velocity;

        private int targetZoomIndex = 1;
        private float[] zoomSteps = new float[] {0.5f, 1f, 2f, 5f};

        private EntityPlayer localPlayer;
        private XUiView xuiTexture;

        private DictionarySave<long, MapObject> keyToMapObject = new DictionarySave<long, MapObject>();
        private DictionarySave<int, NavObject> keyToNavObject = new DictionarySave<int, NavObject>();
        private DictionarySave<int, GameObject> keyToNavSprite = new DictionarySave<int, GameObject>();
        private DictionarySave<long, GameObject> keyToMapSprite = new DictionarySave<long, GameObject>();

        private Dictionary<long, uint[]> mapDataCache = new Dictionary<long, uint[]>();

        private uint[] emptyChunk = new uint[128];
        private uint[] mapColorsData;

        private ComputeBuffer mapDataBuffer;
        private ComputeShader mapGenShader;

        private int kernelIndex;

        private HashSetLong navObjectsOnMapAlive = new HashSetLong();
        private HashSetLong mapObjectsOnMapAlive = new HashSetLong();

        private GameObject prefabMapSprite;

        private Transform transformSpritesParent;

        private bool isOpen;
        private float mapScale = 1f;

        public override void Init()
        {
            base.Init();

            //var maxSizeMb = SystemInfo.maxGraphicsBufferSize / 1024 / 1024;
            //Logging.Inform($"Maximum graphics buffer size is {maxSizeMb} MB");

            if (mapTextureRender == null)
            {
                mapTextureRender = new RenderTexture(MapDrawnSize, MapDrawnSize, 0, RenderTextureFormat.ARGB32);
                mapTextureRender.name = "MinimapRT";
                mapTextureRender.wrapMode = TextureWrapMode.Clamp;
                mapTextureRender.enableRandomWrite = true;
                mapTextureRender.Create();
            }

            if (mapDataBuffer == null)
            {
                mapDataBuffer = new ComputeBuffer(MapDrawnSize * MapDrawnSize, 4, ComputeBufferType.Default);
            }

            if (mapGenShader == null)
            {
                mapGenShader = LoadComputeShader();
                kernelIndex = mapGenShader.FindKernel("DoublePack");
                mapGenShader.SetTexture(kernelIndex, "Minimap", mapTextureRender);
                mapGenShader.SetBuffer(kernelIndex, "data", mapDataBuffer);
                mapGenShader.SetInt("width", bufferRowLength);
            }

            if (mapColorsData == null)
            {
                mapColorsData = new uint[MapDrawnSize * MapDrawnSize / 2];
            }

            XUiController childById = GetChildById("mapViewTexture");
            if (childById != null)
            {
                xuiTexture = childById.ViewComponent;
            }

            transformSpritesParent = GetChildById("clippingPanel").ViewComponent.UiTransform;

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
            uint emptyBlocks = PackShorts(a, a);
            for (int i = 0; i < emptyChunk.Length; i++)
            {
                emptyChunk[i] = emptyBlocks;
            }

            ResetMapColorsData();
        }

        private void Instance_OnNavObjectRemoved(NavObject newNavObject)
        {
            UnityEngine.Object.Destroy(keyToNavSprite[newNavObject.Key]);
            keyToNavObject.Remove(newNavObject.Key);
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

            if(QuartzInputManager.minimapActions.MinimapZoomIn.WasPressed)
            {
                MapZoomed(targetZoomIndex - 1);
            }

            if(QuartzInputManager.minimapActions.MinimapZoomOut.WasPressed)
            {
                MapZoomed(targetZoomIndex + 1);
            }

            SetZoomLevel();

            if (bShouldRedrawMap)
            {
                UpdateFullMap();
                bShouldRedrawMap = false;
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

        public override bool ParseAttribute(string attribute, string value, XUiController _parent)
        {
            switch (attribute)
            {
                case "zoomsteps":
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
                default:
                    return base.ParseAttribute(attribute, value, _parent);
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
                    texture.Material.SetTexture("_MainTex", mapTextureRender);
                    texture.Material.shader = LoadMinimapShader();
                }

                if (xuiTexture is XUiV_MaskedTexture maskedTexture)
                {
                    maskedTexture.Material.SetTexture("_MainTex", mapTextureRender);
                    maskedTexture.Material.shader = LoadMinimapShader();
                }

                cTexMiddle = xuiTexture.Size / 2;
            }
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

            int mapStartX = (int)mapMiddlePosChunks.x - mapUpdateSizeRadius;
            int mapEndX = (int)mapMiddlePosChunks.x + mapUpdateSizeRadius;
            int mapStartZ = (int)mapMiddlePosChunks.y - mapUpdateSizeRadius;
            int mapEndZ = (int)mapMiddlePosChunks.y + mapUpdateSizeRadius;

            MicroStopwatch stopwatch = new MicroStopwatch();
            stopwatch.Start();

            UpdateMapSectionCompute(mapStartX, mapStartZ, mapEndX, mapEndZ, (MapDrawnSize / 2) - mapUpdateSizeRadius, (MapDrawnSize / 2) - mapUpdateSizeRadius, 512, 512);

            stopwatch.Stop();

            Logging.Inform(TAG, "updateFullMap Called, time taken = " + (stopwatch.ElapsedMicroseconds * 0.001d));

            PositionMapAtPlayer();
            SendMapPositionToServer();
        }

        private void UpdateMapSectionCompute(int mapStartX, int mapStartY, int mapEndX, int mapEndY, int drawnMapStartX, int drawnMapStartY, int drawnMapEndX, int drawnMapEndY)
        {
            MapChunkDatabase mapDatabase = localPlayer.ChunkObserver.mapDatabase;
            int num = mapStartY;
            int y = drawnMapStartY;
            while (num < mapEndY)
            {
                int num3 = mapStartX;
                int x = drawnMapStartX;
                while (num3 < mapEndX)
                {
                    int num5 = World.toChunkXZ(num3);
                    int num6 = World.toChunkXZ(num);

                    long chunkKey = WorldChunkCache.MakeChunkKey(num5, num6);
                    ushort[] mapColors = mapDatabase.GetMapColors(chunkKey);
                    int indexBuffer = ((y / 16) * bufferRowLength) + ((x / 16) * 128);
                    if (mapColors != null)
                    {
                        uint[] cachedChunk;
                        if (!mapDataCache.TryGetValue(chunkKey, out cachedChunk))
                        {
                            cachedChunk = new uint[128];
                            uint value;

                            int textureOffset = 0;
                            for (int i = 0; i < 128; i++, textureOffset +=2)
                            {
                                value = PackShorts(mapColors[textureOffset], mapColors[textureOffset + 1]);
                                mapColorsData[indexBuffer + i] = value;
                                cachedChunk[i] = value;
                            }

                            mapDataCache.Add(chunkKey, cachedChunk);
                        }
                        else
                        {
                            Array.Copy(cachedChunk, 0, mapColorsData, indexBuffer, 128);
                        }
                    }
                    else
                    {
                        Array.Copy(emptyChunk, 0, mapColorsData, indexBuffer, 128);
                    }

                    num3 += 16;
                    x = (x + 16) % MapDrawnSize;
                }

                num += 16;
                y = (y + 16) % MapDrawnSize;
            }


            //TODO: Move out of this method to be excuted on the next frame?
            mapDataBuffer.SetData(mapColorsData);
            mapGenShader.Dispatch(kernelIndex, MapDrawnSizeInChunks, MapDrawnSizeInChunks, 1);
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
            //float xScale = xuiTexture.Size.x / MapOnScreenSize;
            //float yScale = xuiTexture.Size.y / MapOnScreenSize;

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
            //float xScale = xuiTexture.Size.x / MapOnScreenSize;
            //float yScale = xuiTexture.Size.y / MapOnScreenSize;

            float yScale = xuiTexture.Size.y / (float)xuiTexture.Size.x;

            //Shader.SetGlobalVector("_MainMapPosAndScale", new Vector4(mapPos.x, mapPos.y, mapScale * xScale, mapScale * yScale));
            Shader.SetGlobalVector("_MainMapPosAndScale", new Vector4(mapPos.x, mapPos.y, mapScale, mapScale * yScale));
        }

        private void MapZoomIn()
        {
            if(targetZoomScale == minZoomScale)
            {
                return;
            }

            targetZoomScale = targetZoomScale != 1f ? targetZoomScale - 1f : 0.5f;

            int newMapUpdateSizeRadius = (int)(MapUpdateRadiusDefault * targetZoomScale);
            if (newMapUpdateSizeRadius > (MapDrawnSize / 2))
            {
                newMapUpdateSizeRadius = MapDrawnSize / 2;
            }

            bShouldRedrawMap = newMapUpdateSizeRadius != mapUpdateSizeRadius;
            mapUpdateSizeRadius = newMapUpdateSizeRadius;

            Manager.PlayInsidePlayerHead("map_zoom_in");
        }

        private void MapZoomOut()
        {
            if (targetZoomScale == maxZoomScale)
            {
                return;
            }

            targetZoomScale = targetZoomScale != 0.5f ? targetZoomScale + 1f : 1f;

            int newMapUpdateSizeRadius = (int)(MapUpdateRadiusDefault * targetZoomScale);
            if (newMapUpdateSizeRadius > (MapDrawnSize/2))
            {
                newMapUpdateSizeRadius = MapDrawnSize / 2;
            }

            bShouldRedrawMap = newMapUpdateSizeRadius != mapUpdateSizeRadius;
            mapUpdateSizeRadius = newMapUpdateSizeRadius;

            Manager.PlayInsidePlayerHead("map_zoom_out");
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
            int newMapUpdateSizeRadius = (int)(MapUpdateRadiusDefault * zoomScale);
            if (newMapUpdateSizeRadius > (MapDrawnSize / 2))
            {
                newMapUpdateSizeRadius = MapDrawnSize / 2;
            }

            bShouldRedrawMap = newMapUpdateSizeRadius != mapUpdateSizeRadius;
            mapUpdateSizeRadius = newMapUpdateSizeRadius;
        }

        private void UpdateMapObject(EnumMapObjectType _type, long _key, string _name, Vector3 _position, Vector3 _size, GameObject _prefab)
        {
            if (!keyToMapSprite.TryGetValue(_key, out var _value))
            {
                _value = transformSpritesParent.gameObject.AddChild(_prefab);
                _value.GetComponent<UISprite>().depth = 20;
                _value.name = _name;
                _value.GetComponent<UISprite>().depth = 1;
                keyToMapObject[_key] = new MapObject(_type, _position, _key, null, _bSelectable: true);
                keyToMapSprite[_key] = _value;
            }

            if ((bool)_value)
            {
                float num = getSpriteZoomScaleFac() * 4.3f;
                UISprite component = _value.GetComponent<UISprite>();
                component.width = (int)(_size.x * num);
                component.height = (int)(_size.z * num);
                Transform transform = _value.transform;
                transform.localPosition = WorldPosToScreenPos(_position);
                transform.localRotation = Quaternion.identity;
                mapObjectsOnMapAlive.Add(_key);
            }
        }

        private float getSpriteZoomScaleFac()
        {
            return global::Utils.FastClamp(1f / (zoomScale * 2f), 0.02f, 20f);
        }

        private void UpdateNavObjectList()
        {
            List<NavObject> navObjectList = NavObjectManager.Instance.NavObjectList;
            navObjectsOnMapAlive.Clear();
            for (int i = 0; i < navObjectList.Count; i++)
            {
                NavObject navObject = navObjectList[i];
                int key = navObject.Key;
                if (navObject.HasRequirements && navObject.NavObjectClass.IsOnMap(navObject.IsActive))
                {
                    NavObjectMapSettings currentMapSettings = navObject.CurrentMapSettings;
                    GameObject gameObject = null;
                    UISprite uISprite = null;
                    if (!keyToNavObject.ContainsKey(key))
                    {
                        gameObject = transformSpritesParent.gameObject.AddChild(prefabMapSprite);
                        uISprite = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
                        string spriteName = navObject.GetSpriteName(currentMapSettings);
                        uISprite.atlas = base.xui.GetAtlasByName(((UnityEngine.Object)uISprite.atlas).name, spriteName);
                        uISprite.spriteName = spriteName;
                        uISprite.depth = currentMapSettings.Layer;
                        keyToNavObject[key] = navObject;
                        keyToNavSprite[key] = gameObject;
                    }
                    else
                    {
                        gameObject = keyToNavSprite[key];
                    }

                    string displayName = navObject.DisplayName;
                    if (!string.IsNullOrEmpty(displayName))
                    {
                        UILabel component = gameObject.transform.Find("Name").GetComponent<UILabel>();
                        component.text = displayName;
                        component.font = xui.GetUIFontByName("ReferenceFont");
                        component.gameObject.SetActive(value: true);
                        component.color = (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color);
                    }
                    else
                    {
                        gameObject.transform.Find("Name").GetComponent<UILabel>().text = "";
                    }

                    float spriteZoomScaleFac = getSpriteZoomScaleFac();
                    uISprite = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
                    Vector3 vector = currentMapSettings.IconScaleVector * spriteZoomScaleFac;
                    uISprite.width = Mathf.Clamp((int)(cSpriteScale * vector.x), 9, 100);
                    uISprite.height = Mathf.Clamp((int)(cSpriteScale * vector.y), 9, 100);
                    uISprite.color = (navObject.hiddenOnCompass ? Color.grey : (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color));
                    uISprite.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f - navObject.Rotation.y);
                    gameObject.transform.localPosition = WorldPosToScreenPos(navObject.GetPosition() + Origin.position);
                    if (currentMapSettings.AdjustCenter)
                    {
                        gameObject.transform.localPosition += new Vector3(uISprite.width / 2, uISprite.height / 2, 0f);
                    }

                    navObjectsOnMapAlive.Add(key);
                }
            }
        }

        protected virtual void UpdateMapObjects()
        {
            World world = GameManager.Instance.World;
            navObjectsOnMapAlive.Clear();
            mapObjectsOnMapAlive.Clear();

            UpdateNavObjectList();
            foreach (KeyValuePair<int, NavObject> item in keyToNavObject.Dict)
            {
                if (!navObjectsOnMapAlive.Contains(item.Key))
                {
                    keyToNavObject.MarkToRemove(item.Key);
                    keyToNavSprite.MarkToRemove(item.Key);
                }
            }

            foreach (KeyValuePair<long, MapObject> item2 in keyToMapObject.Dict)
            {
                if (!mapObjectsOnMapAlive.Contains(item2.Key))
                {
                    keyToMapObject.MarkToRemove(item2.Key);
                    keyToMapSprite.MarkToRemove(item2.Key);
                }
            }

            keyToNavObject.RemoveAllMarked(delegate (int _key)
            {
                keyToNavObject.Remove(_key);
            });
            keyToNavSprite.RemoveAllMarked(delegate (int _key)
            {
                UnityEngine.Object.Destroy(keyToNavSprite[_key]);
                keyToNavSprite.Remove(_key);
            });
            keyToMapObject.RemoveAllMarked(delegate (long _key)
            {
                keyToMapObject.Remove(_key);
            });
            keyToMapSprite.RemoveAllMarked(delegate (long _key)
            {
                UnityEngine.Object.Destroy(keyToMapSprite[_key]);
                keyToMapSprite.Remove(_key);
            });
            localPlayer.selectedSpawnPointKey = -1L;
        }

        private Vector3 WorldPosToScreenPos(Vector3 _worldPos)
        {
            return new Vector3((_worldPos.x - mapMiddlePosPixel.x) * 2.11904764f / zoomScale + (float)cTexMiddle.x, (_worldPos.z - mapMiddlePosPixel.y) * 2.11904764f / zoomScale - (float)cTexMiddle.y, 0f);
        }

        public uint PackShorts(ushort first, ushort second)
        {
            return ((uint)first << 16) + second;
        }

        public ushort ToColor5(float r, float g, float b)
        {
            return (ushort)(((int)(r * 31f + 0.5f) << 10) | ((int)(g * 31f + 0.5f) << 5) | (int)(b * 31f + 0.5f));
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
            for (int i = 0; i < mapColorsData.Length; i += 128)
            {
                Array.Copy(emptyChunk, 0, mapColorsData, i, 128);
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();

            mapTextureRender.Release();
            mapDataBuffer.Release();
            UnityEngine.Object.Destroy(mapTextureRender);

            mapTextureRender = null;
            mapDataBuffer = null;
            mapDataCache.Clear();
        }

        private ComputeShader LoadComputeShader()
        {
            return DataLoader.LoadAsset<ComputeShader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/MinimapCreation.compute");
        }

        private Shader LoadMinimapShader()
        {
            return DataLoader.LoadAsset<Shader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/MaskedMinimap.shader");
        }
    }
}

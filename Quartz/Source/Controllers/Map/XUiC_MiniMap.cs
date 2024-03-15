using Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quartz
{
    public class XUiC_MiniMap : XUiController
    {
        private const string TAG = "XUiC_Minimap";

        public const int MapDrawnSize = 1024;
        public const int MapUpdateSizeRadius = 128;

        public const int MapDrawnSizeInChunks = 64;

        public int BufferRowLength = MapDrawnSizeInChunks * 128;

        private const float cMinZoomScale = 0.7f;
        private const float cMaxZoomScale = 5f;

        private int cSpriteScale = 50;

        private const int MapOnScreenSize = 712;
        private const int MapSizeFull = 2048;
        private const int MapSizeZoom1 = 336;

        private const float factorScreenSizeToDTM = 2.11904764f;
        private const float dragFactorSizeOfMap = 0.471910119f;

        private RenderTexture mapTextureRender;

        private const byte mapMaskTransparency = byte.MaxValue;

        private Vector2i cTexMiddle = new Vector2i(356, 356);

        private Vector2 mapScrollTextureOffset;
        private int mapScrollTextureChunksOffsetX;
        private int mapScrollTextureChunksOffsetZ;

        private bool bMapInitialized;

        private bool bShouldRedrawMap;
        private float timeToRedrawMap;
        private bool bFowMaskEnabled;

        private Vector2 mapMiddlePosChunks;
        private Vector2 mapMiddlePosPixel;
        private Vector2 mapMiddlePosChunksToServer;

        private float zoomScale;
        private float targetZoomScale;

        private EntityPlayer localPlayer;
        private XUiV_Texture xuiTexture;

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

        private Vector2 mapPos;
        private Vector2 mapBGPos;

        private const float ZOOM_SPEED = 5f;

        public override void Init()
        {
            base.Init();

            //var maxSizeMb = SystemInfo.maxGraphicsBufferSize / 1024 / 1024;
            //Logging.Inform($"Maximum graphics buffer size is {maxSizeMb} MB");

            if (mapTextureRender == null)
            {
                mapTextureRender = new RenderTexture(MapDrawnSize, MapDrawnSize, 0, RenderTextureFormat.ARGB32);
                mapTextureRender.wrapMode = TextureWrapMode.Clamp;
                mapTextureRender.enableRandomWrite = true;
                mapTextureRender.Create();
            }

            if(mapDataBuffer == null)
            {
                mapDataBuffer = new ComputeBuffer(MapDrawnSize * MapDrawnSize, 4, ComputeBufferType.Default);
            }

            if(mapGenShader == null)
            {
                mapGenShader = LoadShader();
                kernelIndex = mapGenShader.FindKernel("DoublePack");
                mapGenShader.SetTexture(kernelIndex, "Minimap", mapTextureRender);
                mapGenShader.SetBuffer(kernelIndex, "data", mapDataBuffer);
                mapGenShader.SetInt("width", BufferRowLength);
            }

            if (mapColorsData == null)
            {
                mapColorsData = new uint[MapDrawnSize * MapDrawnSize/2];
            }

            XUiController childById = GetChildById("mapViewTexture");
            if(childById != null)
            {
                xuiTexture = childById.ViewComponent as XUiV_Texture;
            }
            transformSpritesParent = GetChildById("clippingPanel").ViewComponent.UiTransform;
            zoomScale = 5f;
            targetZoomScale = 5f;
            xui.LoadData("Prefabs/MapSpriteEntity", delegate (GameObject o)
            {
                prefabMapSprite = o;
            });

            bShouldRedrawMap = true;
            initMap();
            NavObjectManager.Instance.OnNavObjectRemoved += Instance_OnNavObjectRemoved;
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
                initMap();
                updateFullMap();
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
                initMap();
            }

            if (bShouldRedrawMap)
            {
                updateFullMap();
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
            updateMapObjects();
        }

        private void initMap()
        {
            if (xui.playerUI.entityPlayer != null)
            {
                localPlayer = xui.playerUI.entityPlayer;
                bMapInitialized = true;
                xuiTexture.Material.SetTexture("_MainTex", mapTextureRender);
                cTexMiddle = xuiTexture.Size / 2;
            }
        }

        private void updateFullMap()
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

            int mapStartX = (int)mapMiddlePosChunks.x - MapUpdateSizeRadius;
            int mapEndX = (int)mapMiddlePosChunks.x + MapUpdateSizeRadius;
            int mapStartZ = (int)mapMiddlePosChunks.y - MapUpdateSizeRadius;
            int mapEndZ = (int)mapMiddlePosChunks.y + MapUpdateSizeRadius;

            MicroStopwatch stopwatch = new MicroStopwatch();
            stopwatch.Start();

            updateMapSectionCompute(mapStartX, mapStartZ, mapEndX, mapEndZ, (MapDrawnSize / 2) - MapUpdateSizeRadius, (MapDrawnSize / 2) - MapUpdateSizeRadius, 512, 512);

            stopwatch.Stop();

            Logging.Inform(TAG, "updateFullMap Called, time taken = " + (stopwatch.ElapsedMicroseconds * 0.001d));

            mapScrollTextureOffset.x = 0f;
            mapScrollTextureOffset.y = 0f;
            mapScrollTextureChunksOffsetX = 0;
            mapScrollTextureChunksOffsetZ = 0;

            PositionMapAtPlayer();


            SendMapPositionToServer();
        }

        private void updateMapSectionCompute(int mapStartX, int mapStartY, int mapEndX, int mapEndY, int drawnMapStartX, int drawnMapStartY, int drawnMapEndX, int drawnMapEndY)
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
                    int indexBuffer = ((y / 16) * BufferRowLength) + ((x / 16) * 128);
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
                            //for (int m = 0; m < 256; m++)
                            //{
                            //    value = PackShorts(255, mapColors[m]);
                            //    mapColorsData[index + m] = value;
                            //    cachedChunk[m] = value;
                            //}

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

        private void positionMap()
        {
            float xScale = xuiTexture.Size.x / 712f;
            float yScale = xuiTexture.Size.y / 712f;

            float numX = (MapDrawnSize - (336f * xScale) * zoomScale) / 2f;
            float numY = (MapDrawnSize - (336f * yScale) * zoomScale) / 2f;

            mapScale = 336f * zoomScale / MapDrawnSize;
            float num2 = (numX + (mapMiddlePosPixel.x - mapMiddlePosChunks.x)) / MapDrawnSize;
            float num3 = (numY + (mapMiddlePosPixel.y - mapMiddlePosChunks.y)) / MapDrawnSize;
            mapPos = new Vector3(num2 + mapScrollTextureOffset.x, num3 + mapScrollTextureOffset.y, 0f);
            mapBGPos.x = (numX + mapMiddlePosPixel.x) / MapDrawnSize;
            mapBGPos.y = (numY + mapMiddlePosPixel.y) / MapDrawnSize;
        }

        private void OnPreRender(LocalPlayerCamera _localPlayerCamera)
        {
            float xScale = xuiTexture.Size.x / 712f;
            float yScale = xuiTexture.Size.y / 712f;
            //Material mapMat = xuiTexture.Material;
            //mapMat.SetVector("_MainMapPosAndScale", new Vector4(mapPos.x, mapPos.y, mapScale * xScale, mapScale * yScale));
            //mapMat.SetVector("_MainMapBGPosAndScale", new Vector4(mapBGPos.x, mapBGPos.y, mapScale * xScale, mapScale * yScale));
            Shader.SetGlobalVector("_MainMapPosAndScale", new Vector4(mapPos.x, mapPos.y, mapScale * xScale, mapScale * yScale));
            Shader.SetGlobalVector("_MainMapBGPosAndScale", new Vector4(mapBGPos.x, mapBGPos.y, mapScale * xScale, mapScale * yScale));
        }

        private void onMapScrolled(XUiController sender, float delta)
        {
            float x = xuiTexture.Size.x / 712f;
            float y = xuiTexture.Size.y / 712f;

            float sizeMax = Mathf.Max(x, y);

            float num = 6f;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                num = 5f * zoomScale;
            }

            float min = 0.7f;
            float max = 6.15f / sizeMax;
            targetZoomScale = global::Utils.FastClamp(zoomScale - delta * num, min, max);
            if (delta < 0f)
            {
                Manager.PlayInsidePlayerHead("map_zoom_in");
            }
            else
            {
                Manager.PlayInsidePlayerHead("map_zoom_out");
            }

        }

        private void updateMapObject(EnumMapObjectType _type, long _key, string _name, Vector3 _position, Vector3 _size, GameObject _prefab)
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
                transform.localPosition = worldPosToScreenPos(_position);
                transform.localRotation = Quaternion.identity;
                mapObjectsOnMapAlive.Add(_key);
            }
        }

        private float getSpriteZoomScaleFac()
        {
            return global::Utils.FastClamp(1f / (zoomScale * 2f), 0.02f, 20f);
        }

        private void updateNavObjectList()
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
                    gameObject.transform.localPosition = worldPosToScreenPos(navObject.GetPosition() + Origin.position);
                    if (currentMapSettings.AdjustCenter)
                    {
                        gameObject.transform.localPosition += new Vector3(uISprite.width / 2, uISprite.height / 2, 0f);
                    }

                    navObjectsOnMapAlive.Add(key);
                }
            }
        }

        protected virtual void updateMapObjects()
        {
            World world = GameManager.Instance.World;
            navObjectsOnMapAlive.Clear();
            mapObjectsOnMapAlive.Clear();

            updateNavObjectList();
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

        private Vector3 worldPosToScreenPos(Vector3 _worldPos)
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

            positionMap();
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

        private ComputeShader LoadShader()
        {
            return DataLoader.LoadAsset<ComputeShader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/MinimapCreation.compute");
        }
    }
}

using Audio;
using InControl;
using System.Collections.Generic;
using UnityEngine;

namespace Quartz
{
    public class XUiC_MiniMap : XUiController
    {
        private const string TAG = "XUiC_Minimap";

        public const int MapDrawnSizeInChunks = 128;
        public const int MapDrawnSize = 1024;
        public const int MapUpdateSizeRadius = 512;

        private const float cMinZoomScale = 0.7f;
        private const float cMaxZoomScale = 5f;

        private int cSpriteScale = 50;

        private const int MapOnScreenSize = 712;
        private const int MapSizeFull = 2048;
        private const int MapSizeZoom1 = 336;

        private const float factorScreenSizeToDTM = 2.11904764f;
        private const float dragFactorSizeOfMap = 0.471910119f;

        private Texture2D mapTexture;

        private byte[] mapColors;

        private const byte mapMaskTransparency = byte.MaxValue;

        private byte[][] fowChunkMaskAlphas = new byte[13][];

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

        private Color32 defaultColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        private Color32 hoverColor = new Color32(222, 206, 163, byte.MaxValue);

        private Color32 disabledColor = new Color32(96, 96, 96, byte.MaxValue);

        private DictionarySave<long, MapObject> keyToMapObject = new DictionarySave<long, MapObject>();
        private DictionarySave<int, NavObject> keyToNavObject = new DictionarySave<int, NavObject>();
        private DictionarySave<int, GameObject> keyToNavSprite = new DictionarySave<int, GameObject>();
        private DictionarySave<long, GameObject> keyToMapSprite = new DictionarySave<long, GameObject>();

        private Dictionary<long, Color32[]> existingMapChunks = new Dictionary<long, Color32[]>();

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

            if (mapTexture == null)
            {
                mapTexture = new Texture2D(MapDrawnSize, MapDrawnSize, TextureFormat.ARGB32, mipChain: false);
            }

            if (mapColors == null)
            {
                if (XUiC_MapArea.poolMapColorsArray.Count == 0)
                {
                    //2048 * 2048 * 4 = 16777216
                    //1024 * 1024 * 4 = 4194304
                    mapColors = new byte[MapDrawnSize * MapDrawnSize * 4];
                }
                else
                {
                    mapColors = XUiC_MapArea.poolMapColorsArray[XUiC_MapArea.poolMapColorsArray.Count - 1];
                    XUiC_MapArea.poolMapColorsArray.RemoveAt(XUiC_MapArea.poolMapColorsArray.Count - 1);
                }
            }

            for (int i = 0; i < mapColors.Length; i += 4)
            {
                mapColors[i] = 0;
                mapColors[i + 1] = 0;
                mapColors[i + 2] = 0;
                mapColors[i + 3] = 0;
            }

            mapTexture.LoadRawTextureData(mapColors);
            mapTexture.Apply();

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
            initFOWChunkMaskColors();

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

        private void initFOWChunkMaskColors()
        {
            xui.LoadData("Textures/UI/fow_chunkMask", delegate (Texture2D o)
            {
                Color32[] pixels = o.GetPixels32();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        byte[] array = new byte[256];
                        int num = 0;
                        for (int k = i * 16; k < (i + 1) * 16; k++)
                        {
                            for (int l = j * 16; l < (j + 1) * 16; l++)
                            {
                                array[num++] = pixels[k * o.width + l].r;
                            }
                        }

                        fowChunkMaskAlphas[i * 3 + j] = array;
                    }
                }

                int num2 = 3;
                for (int m = 0; m < 4; m++)
                {
                    byte[] array2 = new byte[256];
                    int num3 = 0;
                    for (int n = num2 * 16; n < (num2 + 1) * 16; n++)
                    {
                        for (int num4 = m * 16; num4 < (m + 1) * 16; num4++)
                        {
                            array2[num3++] = pixels[n * o.width + num4].r;
                        }
                    }

                    fowChunkMaskAlphas[num2 * 3 + m] = array2;
                }
            });
        }

        public override void OnOpen()
        {
            base.OnOpen();
            if (!isOpen)
            {
                isOpen = true;
                localPlayer = xui.playerUI.entityPlayer;
                bFowMaskEnabled = !GameManager.Instance.IsEditMode();
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
                xuiTexture.Material.SetTexture("_MainTex", mapTexture);
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

            Logging.Inform(TAG, "updateFullMap Called");

            mapMiddlePosChunks = middlePosChunk;

            int mapStartX = (int)mapMiddlePosChunks.x - MapUpdateSizeRadius;
            int mapEndX = (int)mapMiddlePosChunks.x + MapUpdateSizeRadius;
            int mapStartZ = (int)mapMiddlePosChunks.y - MapUpdateSizeRadius;
            int mapEndZ = (int)mapMiddlePosChunks.y + MapUpdateSizeRadius;
            mapTexture.LoadRawTextureData(mapColors);
            updateMapSection(mapStartX, mapStartZ, mapEndX, mapEndZ, (MapDrawnSize/2) - MapUpdateSizeRadius, (MapDrawnSize / 2) - MapUpdateSizeRadius, 512, 512);

            mapScrollTextureOffset.x = 0f;
            mapScrollTextureOffset.y = 0f;
            mapScrollTextureChunksOffsetX = 0;
            mapScrollTextureChunksOffsetZ = 0;

            PositionMapAtPlayer();

            //mapTexture.LoadRawTextureData(mapColors);
            mapTexture.Apply();

            SendMapPositionToServer();
        }

        private void updateMapSection(int mapStartX, int mapStartZ, int mapEndX, int mapEndZ, int drawnMapStartX, int drawnMapStartZ, int drawnMapEndX, int drawnMapEndZ)
        {
            MapChunkDatabase mapDatabase = localPlayer.ChunkObserver.mapDatabase;
            int num = mapStartZ;
            int num2 = drawnMapStartZ;
            while (num < mapEndZ)
            {
                int num3 = mapStartX;
                int num4 = drawnMapStartX;
                while (num3 < mapEndX)
                {
                    int num5 = World.toChunkXZ(num3);
                    int num6 = World.toChunkXZ(num);

                    long chunkKey = WorldChunkCache.MakeChunkKey(num5, num6);
                    ushort[] array = mapDatabase.GetMapColors(chunkKey);
                    if (array != null)
                    {
                        if (existingMapChunks.ContainsKey(chunkKey))
                        {
                            Color32[] chunkColors = existingMapChunks[chunkKey];

                            mapTexture.SetPixels32(num4, num2, 16, 16, chunkColors);
                        }
                        else
                        {

                            bool flag2 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5, num6 + 1));
                            bool flag3 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5, num6 - 1));
                            bool flag4 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 - 1, num6));
                            bool flag5 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 + 1, num6));
                            int num19 = 0;
                            if (flag2 && flag3 && flag4 && flag5)
                            {
                                bool flag6 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 - 1, num6 + 1));
                                bool flag7 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 + 1, num6 + 1));
                                bool flag8 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 - 1, num6 - 1));
                                bool flag9 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 + 1, num6 - 1));
                                num19 = ((!flag6) ? 9 : ((!flag7) ? 10 : ((!flag9) ? 11 : (flag8 ? 4 : 12))));
                            }
                            else
                            {
                                if (flag3 && !flag2)
                                {
                                    num19 += 6;
                                }
                                else if (flag3 && flag2)
                                {
                                    num19 += 3;
                                }

                                if (flag5 && flag4)
                                {
                                    num19++;
                                }
                                else if (flag4)
                                {
                                    num19 += 2;
                                }
                            }

                            byte[] array2 = fowChunkMaskAlphas[num19];
                            if (!bFowMaskEnabled)
                            {
                                array2 = fowChunkMaskAlphas[4];
                            }

                            Color32[] newChunkColors = new Color32[256];

                            for (int i = 0; i < 256; i++)
                            {
                                Color32 color = global::Utils.FromColor5To32(array[i]);
                                color.a = array2[i];
                                newChunkColors[i] = color;

                            }

                            existingMapChunks.Add(chunkKey, newChunkColors);
                            mapTexture.SetPixels32(num4, num2, 16, 16, newChunkColors);

                            //for (int row = 0; row < 16; row++)
                            //{
                            //    int num22 = (num2 + row) * MapDrawnSize;
                            //    int num25 = row * 16;
                            //    for (int col = 0; col < 16; col++)
                            //    {
                            //        int num23 = num4 + col;
                            //        int num24 = (num22 + num23) * 4;
                            //        int arrayIndex = num25 + col;
                            //        mapColors[num24] = array2[arrayIndex];
                            //        Color32 color = global::Utils.FromColor5To32(array[arrayIndex]);
                            //        mapColors[num24 + 1] = color.r;
                            //        mapColors[num24 + 2] = color.g;
                            //        mapColors[num24 + 3] = color.b;
                            //    }

                            //}
                        }
                    }

                    num3 += 16;
                    num4 = (num4 + 16) % MapDrawnSize;
                }

                num += 16;
                num2 = (num2 + 16) % MapDrawnSize;
            }
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
            UnityEngine.Object.Destroy(mapTexture);
            if (mapColors != null && XUiC_MapArea.poolMapColorsArray.Count < 10)
            {
                XUiC_MapArea.poolMapColorsArray.Add(mapColors);
            }
        }
    }
}

/*Copyright 2023 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using Quartz.Views;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Quartz
{
    public class XUiV_MaskedTexture : XUiV_ImageBased
    {
        protected UIMaskedTexture uiTexture;

        protected Texture texture;
        protected Texture mask;

        protected string texturePathName;
        protected string maskPathName;

        protected Material material;

        protected Rect uvRect = new Rect(0f, 0f, 1f, 1f);

        protected Vector4 border = Vector4.zero;

        protected Color color = Color.white;

        protected UIBasicSprite.FillDirection fillDirection;

        protected bool isExternalTexture;
        protected bool isExternalMask;

        public UIMaskedTexture UITexture => uiTexture;

        [XuiXmlAttribute("texture")]
        public string TexturePath
        {
            get
            {
                return texturePathName;
            }

            set
            {
                if (texturePathName == value)
                {
                    return;
                }

                if (string.IsNullOrEmpty(value))
                {
                    texturePathName = null;
                    Texture = null;
                    base.SetDirty();
                    return;
                }
                texturePathName = value;
                LoadTexture(texturePathName, true);
            }
        }

        [XuiXmlAttribute("mask")]
        public string MaskPath
        {
            get
            {
                return maskPathName;
            }

            set
            {
                if (maskPathName == value)
                {
                    return;
                }

                if (string.IsNullOrEmpty(value))
                {
                    maskPathName = null;
                    Mask = null;
                    base.SetDirty();
                    return;
                }
                maskPathName = value;
                LoadTexture(maskPathName, false);
            }
        }

        public Texture Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
                //isDirty = true;
                if (value == null)
                {
                    isExternalTexture = false;
                }
            }
        }

        public Texture Mask
        {
            get
            {
                return mask;
            }
            set
            {
                mask = value;
                //isDirty = true;
                if (value == null)
                {
                    isExternalMask = false;
                }
            }
        }

        public Material Material
        {
            get
            {
                return material;
            }
            set
            {
                material = value;
                isDirty = true;
            }
        }

        public Rect UVRect
        {
            get
            {
                return uvRect;
            }
            set
            {
                uvRect = value;
                isDirty = true;
            }
        }

        public Vector4 Border
        {
            get
            {
                return border;
            }
            set
            {
                border = value;
                isDirty = true;
            }
        }

        [XuiXmlAttribute("color", false)]
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                isDirty = true;
            }
        }

        public UIBasicSprite.FillDirection FillDirection
        {
            get
            {
                return fillDirection;
            }
            set
            {
                fillDirection = value;
                isDirty = true;
            }
        }

        public XUiV_MaskedTexture(XUi _xui, string _id)
        : base(_xui, _id)
        {
        }

        public override void createComponents(GameObject _go)
        {
            _go.AddComponent<UIMaskedTexture>();
        }

        public override void captureComponents()
        {
            base.captureComponents();
            uiTexture = uiTransform.gameObject.GetComponent<UIMaskedTexture>();
            widget = uiTexture;
        }

        public override void InitView()
        {
            base.InitView();
            updateData();
        }

        public override void updateData()
        {
            uiTexture.enabled = texture != null;
            uiTexture.mainTexture = texture;
            uiTexture.maskTexture = mask;
            uiTexture.color = opacityModColor(color);
            uiTexture.keepAspectRatio = keepAspectRatio;
            uiTexture.aspectRatio = aspectRatio;
            uiTexture.SetDimensions(size.x, size.y);
            uiTexture.type = type;
            uiTexture.border = border;
            uiTexture.uvRect = uvRect;
            uiTexture.flip = flip;
            uiTexture.centerType = (fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible);
            uiTexture.fillDirection = fillDirection;
            uiTexture.material = material;

            base.updateData();
        }

        [XuiXmlAttribute("material")]
        public void attributeMaterial(string _value)
        {
            xui.LoadData(_value, delegate (Material o)
            {
                material = new Material(o);
            });
        }

        [XuiXmlAttribute("rect_offset")]
        public void attributeRectOffset(Vector2 _value)
        {
            Rect uvrect = UVRect;
            uvrect.x = _value.x;
            uvrect.y = _value.y;
            UVRect = uvrect;
        }

        [XuiXmlAttribute("rect_size")]
        [PublicizedFrom(EAccessModifier.Private)]
        public void attributeRectSize(Vector2 _value)
        {
            Rect uvrect = UVRect;
            uvrect.width = _value.x;
            uvrect.height = _value.y;
            UVRect = uvrect;
        }

        public void UnloadTexture()
        {
            if (Texture != null)
            {
                Texture assetToUnload = Texture;
                uiTexture.mainTexture = null;
                Texture = null;
                texturePathName = null;
                if (!isExternalTexture)
                {
                    Resources.UnloadAsset(assetToUnload);
                }
            }

            if(Mask != null)
            {
                Texture assetToUnload = Mask;
                uiTexture.maskTexture = null;
                Mask = null;
                maskPathName = null;
                if(!isExternalMask)
                {
                    Resources.UnloadAsset (assetToUnload);
                }
            }
        }

        private void LoadTexture(string path, bool isMainTexture)
        {
            try
            {
                string text = ModManager.PatchModPathString(path);
                if (text != null)
                {
                    fetchWwwTexture("file://" + text, isMainTexture);
                }
                else if (path[0] == '@')
                {
                    string text2 = path.Substring(1);
                    if (text2.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                    {
                        string text3 = text2.Substring(5);
                        if (text3[0] != '/' && text3[0] != '\\')
                        {
                            text2 = new Uri(((Application.platform == RuntimePlatform.OSXPlayer) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + text3).AbsoluteUri;
                        }
                    }

                    fetchWwwTexture(text2, isMainTexture);
                }
                else
                {
                    xui.LoadData(path, delegate (Texture o)
                    {
                        if(isMainTexture)
                        {
                            Texture = o;
                            isExternalTexture = false;
                        }
                        else
                        {
                            Mask = o;
                            isExternalMask = false;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error("[XUi] Could not load texture: " + path);
                Log.Exception(e);
            }
        }

        private void fetchWwwTexture(string _uri, bool isMainTexture)
        {
            _uri = _uri.Replace("#", "%23").Replace("+", "%2B");
            UnityWebRequest texture = UnityWebRequestTexture.GetTexture(_uri);
            texture.SendWebRequest();
            ThreadManager.StartCoroutine(waitForWwwData(texture, _uri, isMainTexture));
        }

        private IEnumerator waitForWwwData(UnityWebRequest _www, string _fetchUri, bool isMainTexture)
        {
            while (!_www.isDone)
            {
                yield return null;
            }
            if (_www.result != UnityWebRequest.Result.Success)
            {
                Logging.Warning("[XUiV_MaskedTexture]", "Retrieving texture file from '" + _fetchUri + "' failed (" + _www.error + ").");
                yield break;
            }
            Texture2D texture = ((DownloadHandlerTexture)_www.downloadHandler).texture;

            if(isMainTexture)
            {
                Texture = TextureUtils.CloneTexture(texture, false, false, true);
                isExternalTexture = true;
            }
            else
            {
                Mask = TextureUtils.CloneTexture(texture, false, false, true);
                isExternalMask = true;
            }

            global::UnityEngine.Object.DestroyImmediate(texture);
            _www.Dispose();
            yield break;
        }
    }
}

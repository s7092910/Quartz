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
    public class XUiV_MaskedTexture : XUiView
    {
        protected UIMaskedTexture uiTexture;

        protected Texture texture;
        protected Texture mask;

        protected string texturePathName;
        protected string maskPathName;

        protected Material material;

        protected Rect uvRect = new Rect(0f, 0f, 1f, 1f);

        protected UIBasicSprite.Type type;

        protected Vector4 border = Vector4.zero;

        protected UIBasicSprite.Flip flip;

        protected Color color = Color.white;

        protected UIBasicSprite.FillDirection fillDirection;

        protected bool fillCenter = true;

        private float globalOpacityModifier = 1f;

        protected UnityWebRequest wwwTexture;
        protected UnityWebRequest wwwMask;

        protected bool wwwAssignedTexture;
        protected bool wwwAssignedMask;

        public UIMaskedTexture UITexture => uiTexture;

        public Texture Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
                isDirty = true;
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
                if(mask != value)
                {
                    mask = value;
                    isDirty = true;
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

        public UIBasicSprite.Type Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
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

        public UIBasicSprite.Flip Flip
        {
            get
            {
                return flip;
            }
            set
            {
                flip = value;
                isDirty = true;
            }
        }

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

        public bool FillCenter
        {
            get
            {
                return fillCenter;
            }
            set
            {
                fillCenter = value;
                isDirty = true;
            }
        }

        public float GlobalOpacityModifier
        {
            get
            {
                return globalOpacityModifier;
            }
            set
            {
                globalOpacityModifier = value;
                isDirty = true;
            }
        }

        public XUiV_MaskedTexture(string _id)
            : base(_id)
        {
        }

        protected override void CreateComponents(GameObject _go)
        {
            _go.AddComponent<UIMaskedTexture>();
        }

        public override void InitView()
        {
            base.InitView();
            uiTexture = uiTransform.gameObject.GetComponent<UIMaskedTexture>();
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (xui.GlobalOpacityChanged)
            {
                isDirty = true;
            }
        }

        public override void UpdateData()
        {
            if (!wwwAssignedTexture && !string.IsNullOrEmpty(texturePathName) && texturePathName.Contains("@"))
            {
                if (!wwwTexture.isDone)
                {
                    return;
                }

                Texture2D texture2D = ((DownloadHandlerTexture)wwwTexture.downloadHandler).texture;
                texture2D.requestedMipmapLevel = 0;
                Texture = texture2D;
                wwwAssignedTexture = true;
            }

            if (!wwwAssignedMask && !string.IsNullOrEmpty(maskPathName) && maskPathName.Contains("@"))
            {
                if (!wwwMask.isDone)
                {
                    return;
                }

                Texture2D texture2D = ((DownloadHandlerTexture)wwwMask.downloadHandler).texture;
                texture2D.requestedMipmapLevel = 0;
                Mask = texture2D;
                wwwAssignedMask = true;
            }

            if (!isDirty)
            {
                return;
            }

            uiTexture.mainTexture = texture;
            uiTexture.maskTexture = mask;
            uiTexture.color = color;
            uiTexture.SetDimensions(size.x, size.y);
            uiTexture.type = type;
            uiTexture.border = border;
            uiTexture.uvRect = uvRect;
            uiTexture.flip = flip;
            uiTexture.centerType = (fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible);
            uiTexture.fillDirection = fillDirection;
            uiTexture.material = material;
            if (globalOpacityModifier != 0f && xui.ForegroundGlobalOpacity < 1f)
            {
                float a = Mathf.Clamp01(color.a * (globalOpacityModifier * xui.ForegroundGlobalOpacity));
                uiTexture.color = new Color(color.r, color.g, color.b, a);
            }

            if (!initialized)
            {
                uiTexture.pivot = pivot;
                uiTexture.depth = depth;
                uiTransform.localScale = Vector3.one;
                uiTransform.localPosition = new Vector3(position.x, position.y, 0f);
                if (EventOnHover || EventOnPress || EventOnScroll || EventOnDrag)
                {
                    BoxCollider boxCollider = collider;
                    boxCollider.center = uiTexture.localCenter;
                    boxCollider.size = new Vector3(uiTexture.localSize.x * colliderScale, uiTexture.localSize.y * colliderScale, 0f);
                }
            }

            uiTexture.keepAspectRatio = keepAspectRatio;
            uiTexture.aspectRatio = aspectRatio;
            parseAnchors(uiTexture);
            base.UpdateData();
        }

        public void UnloadTexture()
        {
            if (Texture != null)
            {
                Texture assetToUnload = Texture;
                uiTexture.mainTexture = null;
                Texture = null;
                texturePathName = null;
                wwwAssignedTexture = false;
                if (wwwTexture == null)
                {
                    Resources.UnloadAsset(assetToUnload);
                }

                wwwTexture = null;
            }

            if(Mask != null)
            {
                Texture assetToUnload = Mask;
                uiTexture.maskTexture = null;
                Mask = null;
                maskPathName = null;
                wwwAssignedMask = false;
                if(wwwTexture == null && assetToUnload != null)
                {
                    Resources.UnloadAsset (assetToUnload);
                }
            }
        }

        public override bool ParseAttribute(string attribute, string value, XUiController _parent)
        {
            bool flag = base.ParseAttribute(attribute, value, _parent);
            if (!flag)
            {
                switch (attribute)
                {
                    case "texture":
                        if (texturePathName == value)
                        {
                            break;
                        }

                        texturePathName = value;
                        try
                        {
                            wwwAssignedTexture = false;
                            string text = ModManager.PatchModPathString(texturePathName);
                            if (text != null)
                            {
                                fetchWwwTexture("file://" + text);
                            }
                            else if (texturePathName[0] == '@')
                            {
                                string text2 = texturePathName.Substring(1);
                                if (text2.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                                {
                                    string text3 = text2.Substring(5);
                                    if (text3[0] != '/' && text3[0] != '\\')
                                    {
                                        text2 = new Uri(((Application.platform == RuntimePlatform.OSXPlayer) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + text3).AbsoluteUri;
                                    }
                                }

                                fetchWwwTexture(text2);
                            }
                            else
                            {
                                xui.LoadData(texturePathName, delegate (Texture o)
                                {
                                    Texture = o;
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("[XUi] Could not load texture: " + texturePathName);
                            Log.Exception(e);
                        }

                        break;
                    case "mask":
                        if (maskPathName == value)
                        {
                            break;
                        }

                        maskPathName = value;
                        try
                        {
                            wwwAssignedMask = false;
                            string text = ModManager.PatchModPathString(maskPathName);
                            if (text != null)
                            {
                                fetchWwwMask("file://" + text);
                            }
                            else if (maskPathName[0] == '@')
                            {
                                string text2 = maskPathName.Substring(1);
                                if (text2.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                                {
                                    string text3 = text2.Substring(5);
                                    if (text3[0] != '/' && text3[0] != '\\')
                                    {
                                        text2 = new Uri(((Application.platform == RuntimePlatform.OSXPlayer) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + text3).AbsoluteUri;
                                    }
                                }

                                fetchWwwMask(text2);
                            }
                            else
                            {
                                xui.LoadData(maskPathName, delegate (Texture o)
                                {
                                    Mask = o;
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("[XUi] Could not load mask texture: " + maskPathName);
                            Log.Exception(e);
                        }

                        break;

                    case "material":
                        xui.LoadData(value, delegate (Material o)
                        {
                            material = new Material(o);
                        });
                        break;
                    case "rect_offset":
                        {
                            Vector2 vector2 = StringParsers.ParseVector2(value);
                            Rect uVRect2 = uvRect;
                            uVRect2.x = vector2.x;
                            uVRect2.y = vector2.y;
                            UVRect = uVRect2;
                            break;
                        }
                    case "rect_size":
                        {
                            Vector2 vector = StringParsers.ParseVector2(value);
                            Rect uVRect = uvRect;
                            uVRect.width = vector.x;
                            uVRect.height = vector.y;
                            UVRect = uVRect;
                            break;
                        }
                    case "type":
                        type = EnumUtils.Parse<UIBasicSprite.Type>(value, _ignoreCase: true);
                        break;
                    case "globalopacity":
                        if (!StringParsers.ParseBool(value))
                        {
                            GlobalOpacityModifier = 0f;
                        }

                        break;
                    case "globalopacitymod":
                        GlobalOpacityModifier = StringParsers.ParseFloat(value);
                        break;
                    default:
                        return false;
                }

                return true;
            }

            return flag;
        }

        private void fetchWwwTexture(string _uri)
        {
            _uri = _uri.Replace("#", "%23").Replace("+", "%2B");
            wwwTexture = UnityWebRequestTexture.GetTexture(_uri);
            wwwTexture.SendWebRequest();
            ThreadManager.StartCoroutine(waitForWwwTextureData());
        }

        private void fetchWwwMask(string _uri)
        {
            _uri = _uri.Replace("#", "%23").Replace("+", "%2B");
            wwwMask = UnityWebRequestTexture.GetTexture(_uri);
            wwwMask.SendWebRequest();
            ThreadManager.StartCoroutine(waitForWwwMaskData());
        }

        private IEnumerator waitForWwwTextureData()
        {
            while (wwwTexture != null && !wwwTexture.isDone)
            {
                yield return null;
            }

            if (wwwTexture != null)
            {
                isDirty = true;
            }
        }

        private IEnumerator waitForWwwMaskData()
        {
            while (wwwMask != null && !wwwMask.isDone)
            {
                yield return null;
            }

            if (wwwMask != null)
            {
                isDirty = true;
            }
        }
    }
}

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

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Quartz
{
    public class XUiV_MaskedPanel : XUiView
    {
        protected UIPanel panel;

        protected Vector2 clippingSize = Vector2.negativeInfinity;
        protected Vector2 clippingCenter = Vector2.negativeInfinity;

        protected string maskPathName;
        protected Texture2D mask;

        protected UnityWebRequest wwwMask;
        protected bool wwwAssignedMask;

        public Vector2 ClippingSize
        {
            get
            {
                return clippingSize;
            }
            set
            {
                if (value != clippingSize)
                {
                    clippingSize = value;
                    isDirty = true;
                }
            }
        }

        public Vector2 ClippingCenter
        {
            get
            {
                return clippingCenter;
            }
            set
            {
                if (value != clippingCenter)
                {
                    clippingCenter = value;
                    isDirty = true;
                }
            }
        }

        public Texture2D Mask
        {
            get
            {
                return mask;
            }
            set
            {
                if (mask != value)
                {
                    mask = value;
                    isDirty = true;
                }
            }
        }

        public XUiV_MaskedPanel(string id) : base(id)
        {
        }

        protected override void CreateComponents(GameObject go)
        {
            go.AddComponent<UIPanel>();
        }

        public override void InitView()
        {
            base.InitView();
            panel = uiTransform.gameObject.GetComponent<UIPanel>();
            panel.clipping = UIDrawCall.Clipping.TextureMask;

            BoxCollider collider = this.collider;
            if (collider != null)
            {
                float centerX = size.x * 0.5f;
                float centerY = size.y * 0.5f;
                collider.center = new Vector3(centerX, -centerY, 0f);
                collider.size = new Vector3(size.x * colliderScale, size.y * colliderScale, 0f);
            }

            if (clippingSize == Vector2.negativeInfinity)
            {
                clippingSize.x = size.x;
                clippingSize.y = size.y;
            }

            if(clippingCenter == Vector2.negativeInfinity)
            {
                clippingCenter.x = size.x / 2f;
                clippingCenter.y = -size.y / 2f;
            }

            isDirty = true;
        }

        public override void UpdateData()
        {
            if (!wwwAssignedMask && !string.IsNullOrEmpty(maskPathName) && maskPathName.Contains("@"))
            {
                if (!wwwMask.isDone)
                {
                    return;
                }

                Texture2D texture2D = ((DownloadHandlerTexture)wwwMask.downloadHandler).texture;
                texture2D.wrapMode = TextureWrapMode.Clamp;
                texture2D.requestedMipmapLevel = 0;
                Mask = texture2D;
                wwwAssignedMask = true;
            }


            if (isDirty)
            {
                panel.depth = depth;
                updateClipping();
            }
            base.UpdateData();
        }

        private void updateClipping()
        {

            if(mask != null && panel.clipTexture != mask)
            {
                panel.clipTexture = mask;
            }

            if (clippingSize.x < 0f)
            {
                clippingSize.x = size.x;
            }
            if (clippingSize.y < 0f)
            {
                clippingSize.y = size.y;
            }

            Vector4 vector = new Vector4(clippingCenter.x, clippingCenter.y, clippingSize.x, clippingSize.y);
            if (panel.baseClipRegion != vector)
            {
                panel.baseClipRegion = vector;
            }
        }

        public override bool ParseAttribute(string attribute, string value, XUiController _parent)
        {
            switch (attribute)
            {
                case "mask":
                    if (maskPathName == value)
                    {
                        return true;
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
                            xui.LoadData(maskPathName, delegate (Texture2D o)
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

                    return true;
                case "clippingsize":
                    clippingSize = StringParsers.ParseVector2(value);
                    return true;
                case "clippingcenter":
                    clippingCenter = StringParsers.ParseVector2(value);
                    return true;

                default:
                    return base.ParseAttribute(attribute, value, _parent);
            }
        }

        private void fetchWwwMask(string _uri)
        {
            _uri = _uri.Replace("#", "%23").Replace("+", "%2B");
            wwwMask = UnityWebRequestTexture.GetTexture(_uri);
            wwwMask.SendWebRequest();
            ThreadManager.StartCoroutine(waitForWwwMaskData());
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

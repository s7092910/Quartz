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
    public class XUiV_MaskedPanel : XUiV_Panel
    {
        protected string maskPathName;
        protected Texture2D mask;

        protected bool isExternalMask;

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
                LoadTexture(maskPathName);
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

        public XUiV_MaskedPanel(XUi _xui, string _id)
        : base(_xui, _id)
        {
        }

        public override void InitView()
        {
            clipping = UIDrawCall.Clipping.TextureMask;
            base.InitView();
        }

        public override void updateData()
        {
            panel.clipTexture = mask;
            base.updateData();
        }

        private void LoadTexture(string path)
        {
            try
            {
                string text = ModManager.PatchModPathString(path);
                if (text != null)
                {
                    fetchWwwTexture("file://" + text);
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

                    fetchWwwTexture(text2);
                }
                else
                {
                    xui.LoadData(path, delegate (Texture2D o)
                    {
                        Mask = o;
                        isExternalMask = false;
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error("[XUi] Could not load texture: " + path);
                Log.Exception(e);
            }
        }

        private void fetchWwwTexture(string _uri)
        {
            _uri = _uri.Replace("#", "%23").Replace("+", "%2B");
            UnityWebRequest texture = UnityWebRequestTexture.GetTexture(_uri);
            texture.SendWebRequest();
            ThreadManager.StartCoroutine(waitForWwwData(texture, _uri));
        }

        private IEnumerator waitForWwwData(UnityWebRequest _www, string _fetchUri)
        {
            while (!_www.isDone)
            {
                yield return null;
            }
            if (_www.result != UnityWebRequest.Result.Success)
            {
                Logging.Warning("[XUiV_MaskedPanel]", "Retrieving texture file from '" + _fetchUri + "' failed (" + _www.error + ").");
                yield break;
            }
            Texture2D texture = ((DownloadHandlerTexture)_www.downloadHandler).texture;
            texture.wrapMode = TextureWrapMode.Clamp;

            Mask = TextureUtils.CloneTexture(texture, false, false, true);
            isExternalMask = true;

            global::UnityEngine.Object.DestroyImmediate(texture);
            _www.Dispose();
            yield break;
        }
    }
}

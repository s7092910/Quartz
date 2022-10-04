/*Copyright 2022 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using System.Globalization;
using UnityEngine;
using UnityEngine.Video;

namespace Quartz
{
    public class XUiV_VideoPlayer : XUiView
    {
        protected VideoPlayer videoPlayer;
        protected UITexture uiTexture;
        protected RenderTexture renderTexture;

        protected string pathName;
        protected bool videoChanged;

        protected bool loopVideo = true;
        protected bool restartOnOpen = false;
        protected bool playOnOpen = true;
        protected bool autoplay = true;

        private float globalOpacityModifier = 1f;

        public VideoPlayer VideoPlayer
        {
            get { return videoPlayer; }
        }

        public string VideoPath
        {
            get
            {
                return pathName;
            }
            set
            {
                if (value != pathName)
                {
                    pathName = value;
                    videoChanged = true;
                    isDirty = true;
                }
            }
        }

        public bool LoopVideo
        {
            get
            {
                return loopVideo;
            }
            set
            {
                if (value != loopVideo)
                {
                    loopVideo = value;
                    isDirty = true;
                }
            }
        }

        public bool RestartOnOpen
        {
            get { return restartOnOpen; }
            set { restartOnOpen = value; }
        }

        public bool AutoPlayOnOpen
        {
            get { return playOnOpen; }
            set { playOnOpen = value; }
        }

        public bool AutoPlay
        {
            get { return autoplay; }
            set { autoplay = value; }
        }

        public float GlobalOpacityModifier
        {
            get
            {
                return globalOpacityModifier;
            }
            set
            {
                if(value != globalOpacityModifier)
                {
                    globalOpacityModifier = value;
                    isDirty = true;
                }
            }
        }

        public XUiV_VideoPlayer(string _id) : base(_id)
        {
        }

        protected override void CreateComponents(GameObject go)
        {
            go.AddComponent<UITexture>();
            go.AddComponent<VideoPlayer>();
        }

        public override void InitView()
        {
            base.InitView();
            uiTexture = uiTransform.gameObject.GetComponent<UITexture>();
            videoPlayer = uiTransform.gameObject.GetComponent<VideoPlayer>();

            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;

            renderTexture = new RenderTexture(size.x, size.y, 32);
            renderTexture.format = RenderTextureFormat.ARGB32;

            videoPlayer.targetTexture = renderTexture;
            uiTexture.mainTexture = renderTexture;

            UpdateData();
            initialized = true;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (xui.GlobalOpacityChanged)
            {
                isDirty = true;
            }
        }

        public override void UpdateData()
        {
            if (!isDirty)
            {
                return;
            }

            uiTexture.SetDimensions(size.x, size.y);

            videoPlayer.isLooping = loopVideo;

            if (globalOpacityModifier != 0f && xui.ForegroundGlobalOpacity < 1f)
            {
                float a = Mathf.Clamp01(globalOpacityModifier * xui.ForegroundGlobalOpacity);
                videoPlayer.targetCameraAlpha = a;
            }

            if (!initialized)
            {
                uiTexture.pivot = pivot;
                uiTexture.depth = depth;
                uiTransform.localScale = Vector3.one;
                uiTransform.localPosition = new Vector3(position.x, position.y, 0f);
                if (EventOnHover || EventOnPress || EventOnScroll || EventOnDrag)
                {
                    BoxCollider collider = this.collider;
                    collider.center = uiTexture.localCenter;
                    collider.size = new Vector3(uiTexture.localSize.x * colliderScale, uiTexture.localSize.y * colliderScale, 0f);
                }
            }

            uiTexture.keepAspectRatio = keepAspectRatio;
            uiTexture.aspectRatio = aspectRatio;
            parseAnchors(uiTexture);
            base.UpdateData();

            if (videoChanged)
            {
                if(!string.IsNullOrEmpty(pathName))
                {
                    videoPlayer.url = pathName;

                    if(autoplay)
                    {
                        videoPlayer.Play();
                    }
                } 
                else
                {
                    Stop();
                }

                videoChanged = false;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            if(restartOnOpen)
            {
                videoPlayer.frame = 0;
            }
            
            if(playOnOpen)
            {
                Play();
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            
            Pause();
            renderTexture.Release();
        }

        public override bool ParseAttribute(string attribute, string value, XUiController parent)
        {
            switch(attribute)
            {
                case "video":
                    string videoPath = ModManager.PatchModPathString(value);
                    VideoPath = videoPath != null ? videoPath : value;
                    return true;
                case "restartonopen":
                    restartOnOpen = StringParsers.ParseBool(value);
                    return true;
                case "autoplayonopen":
                    playOnOpen = StringParsers.ParseBool(value);
                    return true;
                case "autoplay":
                    autoplay = StringParsers.ParseBool(value);
                    return true;
                case "loop":
                    LoopVideo = StringParsers.ParseBool(value);
                    return true;
                case "globalopacitymod":
                    GlobalOpacityModifier = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
                    return true;
                default:
                    return base.ParseAttribute(attribute, value, parent);
            }
        }

        public void Play()
        {
            if(initialized && !string.IsNullOrEmpty(pathName) && !videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
        }

        public void Pause()
        {
            if (initialized && videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
        }

        public void Stop()
        {
            if (initialized && videoPlayer.isPrepared)
            {
                videoPlayer.Stop();
                renderTexture.Release();
            }
        }
    }
}

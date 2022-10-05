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

        private const string TAG = "XUiV_VideoPlayer";

        protected VideoPlayer videoPlayer;
        protected UITexture uiTexture;
        protected RenderTexture renderTexture;

        protected string videoPath;
        protected bool videoDirty;

        protected bool loopVideo = true;
        protected bool restartOnOpen = false;
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
                return videoPath;
            }
            set
            {
                if (value != videoPath)
                {
                    videoPath = value;
                    videoDirty = true;
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
                if (value != globalOpacityModifier)
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
            videoPlayer.errorReceived += VideoPlayer_errorReceived;

            renderTexture = new RenderTexture(size.x, size.y, 32);
            renderTexture.format = RenderTextureFormat.ARGB32;

            videoPlayer.targetTexture = renderTexture;
            uiTexture.mainTexture = renderTexture;
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
            renderTexture.width = size.x;
            renderTexture.height = size.y;

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

                initialized = true;
            }

            uiTexture.keepAspectRatio = keepAspectRatio;
            uiTexture.aspectRatio = aspectRatio;
            parseAnchors(uiTexture);
            base.UpdateData();

            if (!string.IsNullOrEmpty(videoPath))
            {
                if (videoDirty)
                {
                    videoPlayer.url = videoPath;
                    videoDirty = false;
                }

                if (!videoPlayer.isPlaying)
                {
                    videoPlayer.Play();
                    if (!autoplay)
                    {
                        videoPlayer.Pause();
                    }
                }
            }
            else
            {
                Stop();
            }

        }

        public override void OnOpen()
        {
            base.OnOpen();
            if(restartOnOpen)
            {
                videoPlayer.frame = 0;
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            
            Pause();
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

        public override void Cleanup()
        {
            base.Cleanup();
            Stop();
        }

        public void Play()
        {
            if(!string.IsNullOrEmpty(videoPath) && !videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
        }

        public void Pause()
        {
            videoPlayer.Pause();
            renderTexture.Release();
        }

        public void Stop()
        {
            videoPlayer.Stop();
            renderTexture.Release();
        }

        private void VideoPlayer_errorReceived(VideoPlayer source, string message)
        {
            Logging.Error(TAG, message);
        }
    }
}

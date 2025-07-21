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

using System;
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

        public VideoPlayer VideoPlayer
        {
            get { return videoPlayer; }
        }

        public event OnVideoFinishedPlayingEvent OnVideoFinishedPlaying;

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

        public XUiV_VideoPlayer(string _id) : base(_id)
        {
        }

        public override void CreateComponents(GameObject go)
        {
            go.AddComponent<UITexture>();
            go.AddComponent<VideoPlayer>();
        }

        public override void InitView()
        {
            base.InitView();
            uiTexture = uiTransform.GetComponent<UITexture>();
            videoPlayer = uiTransform.GetComponent<VideoPlayer>();

            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.errorReceived += VideoPlayer_errorReceived;
            videoPlayer.loopPointReached += VideoPlayer_loopPointReached;

            ModEvents.GameShutdown.RegisterHandler(OnShutdown);
        }

        private void OnShutdown(ref ModEvents.SGameShutdownData _data)
        {
            videoPlayer.Pause();
        }

        public override void UpdateData()
        {
            if (!isDirty)
            {
                return;
            }

            uiTexture.SetDimensions(size.x, size.y);

            if(renderTexture != null)
            {
                renderTexture.Release();
                renderTexture = new RenderTexture(size.x, size.y, 32);
                renderTexture.format = RenderTextureFormat.ARGB32;

                videoPlayer.targetTexture = renderTexture;
                uiTexture.mainTexture = renderTexture;
            } 
            else
            {
                renderTexture = new RenderTexture(size.x, size.y, 32);
                renderTexture.format = RenderTextureFormat.ARGB32;

                videoPlayer.targetTexture = renderTexture;
                uiTexture.mainTexture = renderTexture;
            }

            videoPlayer.isLooping = loopVideo;

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
                default:
                    return base.ParseAttribute(attribute, value, parent);
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            ModEvents.GameShutdown.UnregisterHandler(OnShutdown);
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
            renderTexture.Release();
            videoPlayer.Pause();
        }

        public void Stop()
        {
            if (videoPlayer != null)
            {
                renderTexture.Release();
                videoPlayer.Stop();
            }
        }

        private void VideoPlayer_errorReceived(VideoPlayer source, string message)
        {
            Logging.Error(TAG, message);
        }

        private void VideoPlayer_loopPointReached(VideoPlayer source)
        {
            OnVideoFinishedPlaying?.Invoke(this);
        }

        public delegate void OnVideoFinishedPlayingEvent(XUiV_VideoPlayer videoPlayer);
    }
}

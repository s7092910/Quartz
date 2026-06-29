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

using HarmonyLib;
using UnityEngine;

namespace Quartz
{
    public class XUiV_AnimatedSprite : XUiV_Sprite
    {

        private const string TAG = "AnimatedSprite";

        protected UISpriteAnimation animation;

        protected string prefix;
        protected bool loop = true;
        protected int frameRate = 30;

        private bool resetAnimation = false;

        private bool initialized = false;

        [XuiXmlAttribute("spriteprefix")]
        public string SpriteNamePrefix
        {
            get
            {
                return prefix;
            }

            set
            {
                if(prefix != value)
                {
                    prefix = value;
                    isDirty = true;
                    resetAnimation = true;
                }
            }
        }

        [XuiXmlAttribute("loop")]
        public bool Loop
        {
            get { return loop; }
            set
            {
                if(loop != value)
                {
                    loop = value;
                    isDirty = true;
                    resetAnimation = true;
                }
            }
        }

        [XuiXmlAttribute("framerate")]
        public int FrameRate
        {
            get { return frameRate; }
            set
            {
                if(frameRate != value)
                {
                    frameRate = value;
                    isDirty = true;
                }
            }
        }

        public XUiV_AnimatedSprite(XUi _xui, string _id)
        : base(_xui, _id)
        {
        }

        public override void createComponents(GameObject go)
        {
            base.createComponents(go);
            go.AddComponent<UISpriteAnimation>();
        }

        public override void updateData()
        {
            if(animation == null && !initialized)
            {
                animation = uiTransform.GetComponent<UISpriteAnimation>();
                Traverse.Create(animation).Field("mSnap").SetValue(false);
                initialized = true;
            }

            if (!string.IsNullOrEmpty(sprite.spriteName))
            {
                spriteName = sprite.spriteName;
            }

            base.updateData();

            animation.namePrefix = prefix;
            animation.framesPerSecond = frameRate;
            animation.loop = loop;

            if (resetAnimation)
            {
                animation.ResetToBeginning();
                animation.Play();
                resetAnimation = false;
            }
        }

        public void PlayAnimation()
        {
            animation.Play();
        }

        public void PauseAnimation()
        {
            animation.Pause();
        }

        public void ResetAnimation()
        {
            animation.ResetToBeginning();
        }
    }
}

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

using Audio;
using UnityEngine;

namespace Quartz
{
    public class ScrollBar_Button : XUiV_Button
    {
        private const string TAG = "ScrollBar Button";

        private AudioClip xuiSound;

        public ScrollBar_Button(string _id) : base(_id)
        {
        }

        public override void InitView()
        {
            base.InitView();

            UIEventListener uIEventListener = UIEventListener.Get(uiTransform.gameObject);
            uIEventListener.onPress += OnPress;

            EventOnPress = xuiSound == null;
        }

        public override void UpdateData()
        {
            currentColor.a = sprite.alpha;
            base.UpdateData();
            sprite.depth = depth;
        }

        public override void RefreshBoxCollider()
        {
            if (sprite != null && !sprite.autoResizeBoxCollider)
            {
                base.RefreshBoxCollider();
            }
        }

        public override bool ParseAttribute(string attribute, string value, XUiController parent)
        {
            if (attribute != null)
            {
                switch (attribute)
                {
                    case "sound_play_on_press_down":
                        xui.LoadData(value, (AudioClip audioClip) =>
                        {
                            xuiSound = audioClip;
                        });
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, parent); ;
                }
            }
            return false;
        }

        private void OnPress(GameObject go, bool pressed)
        {
            if (enabled && pressed)
            {
                if (xuiSound != null && xuiSound != null && UICamera.currentTouchID == -1)
                {
                    Manager.PlayXUiSound(xuiSound, soundVolume);
                }

                controller.Pressed(UICamera.currentTouchID);
            }
        }
    }
}
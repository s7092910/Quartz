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

using UnityEngine;

namespace Quartz
{
    public class UIScrollBar : global::UIScrollBar
    {
        private const string TAG = "XUi_UIScrollBar";

        public void setBackgroundWidget(UIWidget background)
        {
            if (backgroundWidget != background)
            {
                backgroundWidget = background;

                if (!background.GetComponent<Collider>()) return;

                UIEventListener bgl = UIEventListener.Get(background.gameObject);
                bgl.onPress += OnPressBackground;
                bgl.onDrag += OnDragBackground;
                background.autoResizeBoxCollider = true;
            }
        }

        public void setForegroundWidget(UIWidget foreground)
        {
            if (foregroundWidget != foreground)
            {
                foregroundWidget = foreground;

                if (!foreground.GetComponent<Collider>()) return;

                UIEventListener fgl = UIEventListener.Get(foreground.gameObject);
                fgl.onPress += OnPressForeground;
                fgl.onDrag += OnDragForeground;
                foreground.autoResizeBoxCollider = true;
            }
        }

        protected new void OnPressBackground(GameObject go, bool isPressed)
        {
            if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
            {
                mCam = UICamera.currentCamera;
                value = ScreenToValue(UICamera.lastEventPosition);
                if (!isPressed && onDragFinished != null)
                {
                    onDragFinished();
                }
            }
        }

        protected new void OnDragBackground(GameObject go, Vector2 delta)
        {
            if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
            {
                mCam = UICamera.currentCamera;
                value = ScreenToValue(UICamera.lastEventPosition);
            }
        }

        protected new void OnPressForeground(GameObject go, bool isPressed)
        {
            if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
            {
                mCam = UICamera.currentCamera;
                if (isPressed)
                {
                    mOffset = mFG == null ? 0f : value - ScreenToValue(UICamera.lastEventPosition);
                }
                else if (onDragFinished != null)
                {
                    onDragFinished();
                }
            }
        }

        protected new void OnDragForeground(GameObject go, Vector2 delta)
        {
            if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
            {
                mCam = UICamera.currentCamera;
                value = mOffset + ScreenToValue(UICamera.lastEventPosition);
            }
        }
    }
}
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
using Quartz.Settings;

namespace Quartz
{
    public class XUiC_MinimapStats : XUiController
    {
        protected EntityPlayer localPlayer;

        //cached values
        protected bool isMinimapEnabled;
        protected bool followPlayerView;
        protected float viewRotation;
        protected float iconOpacity;
        protected float textureOpacity;

        public override void Update(float dt)
        {
            base.Update(dt);
            if (!windowGroup.isShowing || !XUi.IsGameRunning() || xui.playerUI.entityPlayer == null)
            {
                return;
            }

            if (xui.playerUI.entityPlayer != null)
            {
                localPlayer = xui.playerUI.entityPlayer;
            }

                bool refreshBindings = false;

            if (isMinimapEnabled != MinimapSettings.Enabled)
            {
                isMinimapEnabled = MinimapSettings.Enabled;
                refreshBindings = true;
            }

            if(followPlayerView != MinimapSettings.FollowPlayerView)
            {
                followPlayerView = MinimapSettings.FollowPlayerView;
                refreshBindings = true;
            }

            float currentRotation = MinimapSettings.FollowPlayerView ? -localPlayer.rotation.y : 0f;
            if (viewRotation != currentRotation)
            {
                viewRotation = currentRotation;
                refreshBindings = true;
            }

            if(iconOpacity != MinimapSettings.IconOpacity)
            {
                iconOpacity = MinimapSettings.IconOpacity;
                refreshBindings = true;
            }

            if (textureOpacity != MinimapSettings.TextureOpacity)
            {
                textureOpacity = MinimapSettings.TextureOpacity;
                refreshBindings = true;
            }

            if (refreshBindings)
            {
                RefreshBindings();
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            RefreshBindings();
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "isminimapenabled":
                    value = isMinimapEnabled.ToString();
                    return true;
                case "isfollowingplayerview":
                    value = followPlayerView.ToString();
                    return true;
                case "viewrotation":
                    value = viewRotation.ToString();
                    return true;
                case "iconopacity":
                    value = iconOpacity.ToString();
                    return true;
                case "textureopacity":
                    value = textureOpacity.ToString();
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }
    }
}

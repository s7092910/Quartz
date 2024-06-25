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

using GearsAPI.Settings.Global;
using UnityEngine;

namespace Quartz.Settings
{
    public class MinimapSettings
    {
        public static IGlobalValueSetting enableMinimapSetting;

        private static bool enabled = true;
        public static bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if(enabled != value)
                {
                    enabled = value;

                    if(enableMinimapSetting != null)
                    {
                        enableMinimapSetting.CurrentValue = enabled ? "Show" : "Hide";
                        QuartzMod.SaveModSettings();
                    }
                }
            }
        }

        public static bool FollowPlayerView { get; set; } = false;

        public static bool ShowIcons { get; set; } = true;

        public static bool ShowMinimapOnlyIcons { get; set; } = true;

        public static bool ShowText { get; set; } = true;

        public static float IconScaleModifer { get; set; } = 1f;

        public static float IconOpacity {  get; set; } = 1f;

        public static float TextureOpacity { get; set; } = 1f;

        public static void SetMinimapEnabled(IGlobalModSetting setting, string newValue)
        {
            enabled = newValue == "Show";
        }

        public static void SetMinimapFollowsPlayerView(IGlobalModSetting setting, string newValue)
        {
            FollowPlayerView = newValue == "On";
        }

        public static void SetIconsEnabled(IGlobalModSetting setting, string newValue)
        {
            ShowIcons = newValue == "Show";
        }

        public static void SetMinimapOnlyIconsEnabled(IGlobalModSetting setting, string newValue)
        {
            ShowMinimapOnlyIcons = newValue == "Show";
        }


        public static void SetTextEnabled(IGlobalModSetting setting, string newValue)
        {
            ShowText = newValue == "Show";
        }

        public static void SetIconScaleModifer(IGlobalModSetting setting, string newValue)
        {
            if(int.TryParse(newValue, out int modifer))
            {
                float scale = modifer / 100f;
                IconScaleModifer = Mathf.Clamp(scale, 0f, 2f);
            }
        }

        public static void SetIconOpacity(IGlobalModSetting setting, string newValue)
        {
            if (int.TryParse(newValue, out int modifer))
            {
                float opacity = modifer / 100f;
                IconOpacity = Mathf.Clamp(opacity, 0f, 1f);
            }
        }

        public static void SetTextureOpacity(IGlobalModSetting setting, string newValue)
        {
            if (int.TryParse(newValue, out int modifer))
            {
                float opacity = modifer / 100f;
                TextureOpacity = Mathf.Clamp(opacity, 0f, 1f);
            }
        }
    }
}

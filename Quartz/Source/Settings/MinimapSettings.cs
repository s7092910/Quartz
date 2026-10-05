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

using GearsAPI.Attributes;
using GearsAPI.Settings;
using GearsAPI.Settings.Global;
using UnityEngine;

namespace Quartz.Settings
{
    public class MinimapSettings
    {
        [Setting("Minimap.Minimap.MinimapShowOrHide")]
        public static IGlobalValueSetting<bool> enableMinimapSetting;

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
                        enableMinimapSetting.SettingValue = enabled;
                        enableMinimapSetting.SelectedValue = enabled;
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


        [SettingOnValueChanged("Minimap.Minimap.MinimapShowOrHide", true)]
        public static void SetMinimapEnabled(IValueModSetting<bool> setting, bool show)
        {
            enabled = show;
        }

        [SettingOnValueChanged("Minimap.Minimap.IconsShowOrHide", true)]
        public static void SetIconsEnabled(IValueModSetting<bool> setting, bool show)
        {
            ShowIcons = show;
        }

        [SettingOnValueChanged("Minimap.Minimap.TextShowOrHide", true)]
        public static void SetTextEnabled(IValueModSetting<bool> setting, bool show)
        {
            ShowText = show;
        }

        [SettingOnValueChanged("Minimap.Minimap.MinimapIconsShowOrHide", true)]
        public static void SetMinimapOnlyIconsEnabled(IValueModSetting<bool> setting, bool show)
        {
            ShowMinimapOnlyIcons = show;
        }

        [SettingOnValueChanged("Minimap.Minimap.RotateWithPlayer", true)]
        public static void SetMinimapFollowsPlayerView(IValueModSetting<bool> setting, bool follow)
        {
            FollowPlayerView = follow;
        }

        [SettingOnValueChanged("Minimap.Minimap.IconScale", true)]
        public static void SetIconScaleModifer(IValueModSetting<float> setting, float scale)
        {
            IconScaleModifer = Mathf.Clamp(scale, 0f, 2f);
        }

        [SettingOnValueChanged("Minimap.Minimap.IconOpacity", true)]
        public static void SetIconOpacity(IValueModSetting<float> setting, float opacity)
        {
            IconOpacity = Mathf.Clamp(opacity, 0f, 1f);
        }

        [SettingOnValueChanged("Minimap.Minimap.TextureOpacity", true)]
        public static void SetTextureOpacity(IValueModSetting<float> setting, float opacity)
        {
            TextureOpacity = Mathf.Clamp(opacity, 0f, 1f);
        }
    }
}

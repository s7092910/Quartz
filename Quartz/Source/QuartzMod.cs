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
using Quartz.Inputs;
using Quartz.Settings;
using System.Reflection;
using GearsAPI.Settings;
using GearsAPI.Settings.Global;
using GearsAPI.Settings.World;
using Quartz.Source.Views.Harmony;

namespace Quartz
{
    public class QuartzMod : IGearsModApi, IModApi
    {

        private const string ModName = "com.Quartz.Mod";

        private static IModGlobalSettings modGlobalSettings;

        public void InitMod(Mod modInstance)
        {
            //If patches have already been loaded, skip.
            if (Harmony.HasAnyPatches(ModName))
            {
                return;
            }

            Logging.Inform("Loading Patch");
            var harmony = new Harmony(ModName);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logging.Inform("Loaded Patch");

            Logging.Inform("Loading ActionSets");
            QuartzInputManager.InitControls(modInstance.Path);

            new QuartzResourcesAPI();
        }

        public void InitMod(IGearsMod modInstance)
        {
        }

        public void OnGlobalSettingsLoaded(IModGlobalSettings modSettings)
        {
            modGlobalSettings = modSettings;

            //General Tab Settings
            IGlobalModSettingsTab tab = modSettings.GetTab("General");
            IGlobalModSettingsCategory cat = tab.GetCategory("General");
            IGlobalValueSetting modSetting = cat.GetSetting("TextResolution") as IGlobalValueSetting;

            modSetting.OnSettingChanged += GlobalSettings.SetTextResolution;
            GlobalSettings.SetTextResolution(modSetting, modSetting.CurrentValue);

            //Minimap
            tab = modSettings.GetTab("Minimap");

            cat = tab.GetCategory("Minimap");

            modSetting = cat.GetSetting("MinimapShowOrHide") as IGlobalValueSetting;
            modSetting.OnSettingChanged += MinimapSettings.SetMinimapEnabled;
            MinimapSettings.SetMinimapEnabled(modSetting, modSetting.CurrentValue);
            MinimapSettings.enableMinimapSetting = modSetting;

            modSetting = cat.GetSetting("IconsShowOrHide") as IGlobalValueSetting;
            modSetting.OnSettingChanged += MinimapSettings.SetIconsEnabled;
            MinimapSettings.SetIconsEnabled(modSetting, modSetting.CurrentValue);

            modSetting = cat.GetSetting("MinimapIconsShowOrHide") as IGlobalValueSetting;
            modSetting.OnSettingChanged += MinimapSettings.SetMinimapOnlyIconsEnabled;
            MinimapSettings.SetMinimapOnlyIconsEnabled(modSetting, modSetting.CurrentValue);

            modSetting = cat.GetSetting("TextShowOrHide") as IGlobalValueSetting;
            modSetting.OnSettingChanged += MinimapSettings.SetTextEnabled;
            MinimapSettings.SetTextEnabled(modSetting, modSetting.CurrentValue);

            modSetting = cat.GetSetting("RotateWithPlayer") as IGlobalValueSetting;
            modSetting.OnSettingChanged += MinimapSettings.SetMinimapFollowsPlayerView;
            MinimapSettings.SetMinimapFollowsPlayerView(modSetting, modSetting.CurrentValue);

            modSetting = cat.GetSetting("IconScale") as IGlobalValueSetting;
            modSetting.OnSettingChanged += MinimapSettings.SetIconScaleModifer;
            MinimapSettings.SetIconScaleModifer(modSetting, modSetting.CurrentValue);

            modSetting = cat.GetSetting("IconOpacity") as IGlobalValueSetting;
            modSetting.OnSettingChanged += MinimapSettings.SetIconOpacity;
            MinimapSettings.SetIconOpacity(modSetting, modSetting.CurrentValue);

            modSetting = cat.GetSetting("TextureOpacity") as IGlobalValueSetting;
            modSetting.OnSettingChanged += MinimapSettings.SetTextureOpacity;
            MinimapSettings.SetTextureOpacity(modSetting, modSetting.CurrentValue);

            //Minimap KeyBindings
            cat = tab.GetCategory("KeyBindings");

            IControlBindingSetting modBinding = cat.GetSetting("EnabledKeyBinding") as IControlBindingSetting;
            modBinding.PlayerAction = QuartzInputManager.minimapActions.MinimapToggle;
            modBinding.OnSettingChanged += ControlsSettingChanged;

            modBinding = cat.GetSetting("ZoomInKeyBinding") as IControlBindingSetting;
            modBinding.PlayerAction = QuartzInputManager.minimapActions.MinimapZoomIn;
            modBinding.OnSettingChanged += ControlsSettingChanged;

            modBinding = cat.GetSetting("ZoomOutKeyBinding") as IControlBindingSetting;
            modBinding.PlayerAction = QuartzInputManager.minimapActions.MinimapZoomOut;
            modBinding.OnSettingChanged += ControlsSettingChanged;

            //Inventory Tab Settings
            tab = modSettings.GetTab("Inventory");
            cat = tab.GetCategory("KeyBindings");
            modBinding = cat.GetSetting("LockedSlots") as IControlBindingSetting;

            modBinding.PlayerAction = QuartzInputManager.inventoryActions.LockSlot;
            modBinding.OnSettingChanged += ControlsSettingChanged;

            //Dev Tools Tab
            tab = modSettings.GetTab("Dev Tools");
            cat = tab.GetCategory("Debug");
            modSetting = cat.GetSetting("DebugMode") as IGlobalValueSetting;

            modSetting.OnSettingChanged += DebuggingSettings.SetDebugMode;
            DebuggingSettings.SetDebugMode(modSetting, modSetting.CurrentValue);
        }

        public void OnWorldSettingsLoaded(IModWorldSettings modSettings)
        {

        }

        private void ControlsSettingChanged(IGlobalModSetting setting, string newValue)
        {
            QuartzInputManager.SaveControls();
        }

        public static void SaveModSettings()
        {
            if(modGlobalSettings != null)
            {
                modGlobalSettings.SaveSettings();
            }
        }
    }
}

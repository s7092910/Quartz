﻿/*Copyright 2022 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using Gears.SettingsManager.Settings;
using Gears.SettingsManager;
using HarmonyLib;
using Quartz.Inputs;
using Quartz.Settings;
using System;
using System.Reflection;

namespace Quartz
{
    public class QuartzMod : IGearsModApi, IModApi
    {

        private const string ModName = "com.Quartz.Mod";

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
        }

        public void OnMainSettingsLoaded(IModSettings modSettings)
        {
            IModSettingsTab tab = modSettings.GetTab("General");

            IModSettingsCategory cat = tab.GetCategory("General");
            IModValueSetting modSetting = cat.GetSetting("TextResolution") as IModValueSetting;

            modSetting.OnSettingChanged += GlobalSettings.SetTextResolution;
            GlobalSettings.SetTextResolution(modSetting, modSetting.CurrentValue);

            cat = tab.GetCategory("Debug");
            modSetting = cat.GetSetting("DebugMode") as IModValueSetting;

            modSetting.OnSettingChanged += GlobalSettings.SetDebugMode;
            GlobalSettings.SetDebugMode(modSetting, modSetting.CurrentValue);

            //Controls Tab Settings
            tab = modSettings.GetTab("Controls");
            cat = tab.GetCategory("Inventory");

            IControlBindingSetting modBinding = cat.GetSetting("LockedSlots") as IControlBindingSetting;

            modBinding.PlayerAction = QuartzInputManager.inventoryActions.LockSlot;
            modBinding.OnSettingChanged += ControlsSettingChanged;
        }

        public void OnWorldSettingsLoaded(IModSettings modSettings)
        {

        }

        private void ControlsSettingChanged(IModSetting setting, string newValue)
        {
            QuartzInputManager.SaveControls();
        }
    }
}

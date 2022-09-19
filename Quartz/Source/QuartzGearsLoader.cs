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

using Gears.ModManager;
using Gears.ModManager.Settings;
using Quartz.Settings;
using Quartz.Source.Inputs;

namespace Quartz
{
    public class QuartzGearsLoader : IGearsModApi
    {
        public void InitMod(IGearsMod modInstance)
        {
            QuartzMod.LoadQuartz(modInstance as Mod);
        }

        public void OnSettingsLoaded(IModSettings modSettings)
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

        private void ControlsSettingChanged(IModSetting setting, string newValue)
        {
            QuartzInputManager.SaveControls();
        }

        public void OnEnabled()
        {

        }

        public void OnDisabled()
        {
            
        }


        public void OnStart()
        {
            
        }

        public void OnStop()
        {
            
        }

        public bool RequireReset()
        {
            return true;
        }
    }
}

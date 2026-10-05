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
            //General Tab Settings
            modInstance.GlobalSettings.BindSettingsClass(typeof(GlobalSettings));

            //Minimap
            modInstance.GlobalSettings.BindSettingsClass(typeof(MinimapSettings));

            //Dev Tools Tab
            modInstance.GlobalSettings.BindSettingsClass(typeof(DebuggingSettings));

            if (GameManager.IsDedicatedServer)
            {
                return;
            }

            //Minimap KeyBindings
            modInstance.GlobalSettings.BindSettingsClass(typeof(MinimapActions));

            //Inventory Tab Settings
            modInstance.GlobalSettings.BindSettingsClass(typeof(InventoryActions));

            //Saving KeyBindings
            modInstance.GlobalSettings.BindSettingsClass(typeof(QuartzInputManager));
        }

        public void OnGlobalSettingsLoaded(IModGlobalSettings modSettings)
        {
            modGlobalSettings = modSettings;
            //General Tab Settings
            modSettings.SyncSettingsToClass(typeof(GlobalSettings));

            //Minimap
            modSettings.SyncSettingsToClass(typeof(MinimapSettings));

            //Dev Tools Tab
            modSettings.SyncSettingsToClass(typeof(DebuggingSettings));

        }

        public void OnWorldSettingsLoaded(IModWorldSettings modSettings)
        {

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

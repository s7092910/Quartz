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
using Quartz.Source.Inputs;
using System;
using System.Reflection;

namespace Quartz
{
    public class QuartzMod
    {
        private const string ModName = "com.Quartz.Mod";

        public static void LoadQuartz(Mod modInstance)
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
            QuartzInputManager.InitCustomControls(modInstance.Path);
        }

    }
}

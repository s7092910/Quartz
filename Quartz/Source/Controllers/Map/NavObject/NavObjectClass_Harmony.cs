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

using HarmonyLib;

namespace Quartz.Map
{
    [HarmonyPatch(typeof(NavObjectClass))]
    public class NavObjectClass_Harmony
    {
        [HarmonyPrefix]
        [HarmonyPatch("IsOnMap")]
        private static bool IsOnMap(NavObjectClass __instance, bool isActive, ref bool __result)
        {
            NavObjectMapSettings mapSettings = __instance.GetMapSettings(isActive);
            __result = mapSettings != null && !(mapSettings.Properties.Contains("minimap_only") && mapSettings.Properties.GetBool("minimap_only"));
            return false;
        }
    }
}

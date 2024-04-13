/*Copyright 2023 Christopher Beda

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
using UnityEngine;

namespace Quartz.Views
{
    [HarmonyPatch(typeof(UIDrawCall))]
    public class UIDrawCall_Harmony
    {
        [HarmonyPostfix]
        [HarmonyPatch("CreateMaterial")]
        public static void CreateMaterial(UIDrawCall __instance, ref Shader ___mShader)
        {
            if(__instance.baseMaterial != null && (__instance.baseMaterial.name.Contains("Transparent FixableMask") || __instance.baseMaterial.shader.name.Equals("Unlit/MaskedMinimap")) && __instance.dynamicMaterial.shader != __instance.baseMaterial.shader)
            {
                __instance.dynamicMaterial.shader = __instance.baseMaterial.shader;
                ___mShader = __instance.baseMaterial.shader;
            }
        }
    }
}

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
using System.Collections.Generic;
using UnityEngine;

namespace Quartz.Views
{
    //[HarmonyPatch(typeof(ResourcesAPI))]
    //public class ResourcesAPI_Harmony
    //{
    //    private static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

    //    [HarmonyPostfix]
    //    [HarmonyPatch("FindShaderByName")]
    //    public static void FindShaderByName(string name, ref Shader __result)
    //    {
    //        if (__result != null)
    //        {
    //            return;
    //        }

    //        if (!shaders.TryGetValue(name, out __result))
    //        {
    //            switch (name)
    //            {
    //                case "Unlit/MaskedMinimap":
    //                    __result = DataLoader.LoadAsset<Shader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/MaskedMinimap.shader");
    //                    shaders.Add(name, __result);
    //                    break;
    //                case "Unlit/Transparent FixableMask":
    //                    __result = DataLoader.LoadAsset<Shader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/UnlitTransparentFixableMask.shader");
    //                    shaders.Add(name, __result);
    //                    break;
    //            }
    //        }
    //    }
    //}
}

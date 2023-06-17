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
using System;

namespace Quartz.Bindings
{
    [HarmonyPatch(typeof(BindingInfo))]
    public class BindingInfo_Harmony
    {

        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(XUiView), typeof(string), typeof(string) })]
        public static void Constructor(BindingInfo __instance, XUiView _view, string _property, string _sourceText)
        {
            //Refreshes the bindingInfo if its sourceText matches a set text.
            //This allows XUiControllers to set the size of the grid on creation so the right amount of rows or columns are created
            if (_sourceText.Contains("spriteCount"))
            {
                __instance.RefreshValue(true);
            }
        }
    }
}

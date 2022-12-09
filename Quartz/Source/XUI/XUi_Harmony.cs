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
using Quartz;
using Quartz.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[HarmonyPatch(typeof(XUi))]
public static class XUiPatch
{
    private const string TAG = "Error Reverse Patching XUiController method: ";

    [HarmonyPrefix]
    [HarmonyPatch("GetUIFontByName")]
    public static bool GetUIFontByName(ref NGUIFont __result, string _name, bool _showWarning = true)
    {

        __result = FontManager.GetNGUIFontByName(_name);

        if (__result == null && _showWarning)
        {
            Log.Warning("XUi font not found: " + _name + ", from: " + StackTraceUtility.ExtractStackTrace());
            Logging.Inform("Defaulting to vanilla font");

            __result = FontManager.GetVanillaFont();
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("LoadAsync")]
    public static bool LoadAsync(ref IEnumerator __result, XUi __instance, List<string> windowGroupSubset = null)
    {
        __result = LoadAsyncInternal(__instance, windowGroupSubset);
        return false;
    }

    [HarmonyReversePatch]
    [HarmonyPatch("LoadAsync")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IEnumerator LoadAsync(XUi instance, List<string> windowGroupSubset = null)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "Update()");
    }

    public static IEnumerator LoadAsyncInternal(XUi xUi, List<string> windowGroupSubset)
    {
        Dictionary<string, XUiFromXml.StyleData> styles = AccessTools.Field(typeof(XUiFromXml), "styles").GetValue(null) as Dictionary<string, XUiFromXml.StyleData>;
        yield return FontManager.LoadFonts(xUi, styles);
        yield return LoadAsync(xUi, windowGroupSubset);
        yield break;
    }
}
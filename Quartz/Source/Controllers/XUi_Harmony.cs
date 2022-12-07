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
using QuartzOverhaul.Extensions;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[HarmonyPatch(typeof(XUi))]
public static class XUiPatch
{

    [HarmonyPrefix]
    [HarmonyPatch("GetUIFontByName")]
    public static bool GetUIFontByName(XUi __instance, ref NGUIFont __result, string _name, bool _showWarning = true)
    {

        __result = __instance.GetNGUIFontByName(_name);

        if (__result == null && _showWarning)
        {
            Log.Warning("XUi font not found: " + _name + ", from: " + StackTraceUtility.ExtractStackTrace());
        }

        return false;

        //if (__instance.NGUIFonts.Length == 1)
        //{
        //    NGUIFont original = __instance.NGUIFonts[0];
        //    NGUIFont ariel = new NGUIFont();

        //    ariel.name = "Ariel";
        //    Font arielFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        //    ariel.dynamicFont= arielFont;

        //    __instance.NGUIFonts = new NGUIFont[] { original, ariel };
        //}

        //for (int i = 0; i < __instance.NGUIFonts.Length; i++)
        //{
        //    if (__instance.NGUIFonts[i].name.EqualsCaseInsensitive(_name) || __instance.NGUIFonts[i].spriteName.EqualsCaseInsensitive(_name))
        //    {
        //        __result = __instance.NGUIFonts[i];
        //        return false;
        //    }
        //}
        //if (_showWarning)
        //{
        //    Log.Warning("XUi font not found: " + _name + ", from: " + StackTraceUtility.ExtractStackTrace());
        //}

        //return false;
    }
}
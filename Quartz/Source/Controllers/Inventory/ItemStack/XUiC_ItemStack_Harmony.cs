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
using UnityEngine;

[HarmonyPatch(typeof(XUiC_ItemStack))]
public class XUiC_ItemStackPatch
{
    private const string TAG = "XUiC_ItemStackPatch";

    [HarmonyPrefix]
    [HarmonyPatch("updateBorderColor")]
    public static bool updateBorderColor(XUiC_ItemStack __instance)
    {
        return !(__instance is Quartz.XUiC_ItemStack);
    }

[HarmonyPostfix]
    [HarmonyPatch("OnHovered")]
    public static void OnHovered(XUiC_ItemStack __instance, bool _isOver, ref bool ___isOver, TweenScale ___tweenScale)
    {
        ___isOver = _isOver;
        if (___tweenScale != null && ___tweenScale.from == Vector3.one)
        {
            ___tweenScale.to = Vector3.one * __instance.HoverIconGrow;
        } 
        else
        {
            ___tweenScale.from = Vector3.one * __instance.HoverIconGrow;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("ResetTweenScale")]
    public static void ResetTweenScale(XUiC_ItemStack __instance, TweenScale ___tweenScale)
    {
        if(___tweenScale != null && ___tweenScale.value != Vector3.one)
        {
            ___tweenScale.from = Vector3.one * __instance.HoverIconGrow;
        }
    }

}

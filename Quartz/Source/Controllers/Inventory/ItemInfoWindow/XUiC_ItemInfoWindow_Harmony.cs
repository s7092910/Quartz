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
using System;
using System.Runtime.CompilerServices;

[HarmonyPatch(typeof(XUiC_ItemInfoWindow))]
public static class XUiC_ItemInfoWindowPatch
{
    private const string TAG = "Error Reverse Patching XUiC_ItemInfoWindow method: ";

    [HarmonyReversePatch]
    [HarmonyPatch("GetStatTitle")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetStatTitle(XUiC_ItemInfoWindow instance, int index)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "GetStatTitle()");
    }

    [HarmonyReversePatch]
    [HarmonyPatch("GetStatValue")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetStatValue(XUiC_ItemInfoWindow instance, int index)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "GetStatValue()");
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetInfo")]
    public static void SetInfo(XUiC_ItemInfoWindow __instance, ItemStack stack, XUiController controller, XUiC_ItemActionList.ItemActionListTypes actionListType, ItemDisplayEntry ___itemDisplayEntry)
    {
        if (__instance is Quartz.XUiC_ItemInfoWindow instance)
        {
            instance.SetItemStats(stack, ___itemDisplayEntry);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("CompareStack", MethodType.Setter)]
    public static void CompareStack(XUiC_ItemInfoWindow __instance)
    {
        if (__instance is Quartz.XUiC_ItemInfoWindow instance)
        {
            instance.RefreshItemStats();
        }
    }
}

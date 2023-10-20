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

using Audio;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System;
using UnityEngine;

[HarmonyPatch(typeof(XUiC_RequiredItemStack))]
public class XUiC_RequiredItemStackPatch
{
    private const string TAG = "XUiC_ItemStackPatch";

    private const string ERRORTAG = "Error Reverse Patching XUiC_RequiredItemStack method: ";

    [HarmonyReversePatch]
    [HarmonyPatch("CanSwap")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool CanSwap(XUiC_RequiredItemStack instance, ItemStack stack)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(ERRORTAG + "emitHeatMapEvent()");
    }

    [HarmonyPrefix]
    [HarmonyPatch("HandleDropOne")]
    public static bool HandleDropOne(XUiC_RequiredItemStack __instance, AudioClip ___placeSound)
    {
        ItemStack currentStack = __instance.xui.dragAndDrop.CurrentStack;

        if (!currentStack.IsEmpty() && XUiC_RequiredItemStackPatch.CanSwap(__instance, currentStack))
        {
            ItemStack itemStack = __instance.ItemStack.Clone();
            if (itemStack.IsEmpty())
            {
                itemStack = currentStack.Clone();
                itemStack.count = 0;
            }

            ItemClass itemClass = currentStack.itemValue.ItemClass;
            int stackMax = ((__instance.OverrideStackCount == -1) ? itemClass.Stacknumber.Value : Mathf.Min(itemClass.Stacknumber.Value, __instance.OverrideStackCount));
            if (itemStack.count + 1 <= stackMax)
            {
                itemStack.count++;
                currentStack.count--;
                __instance.xui.dragAndDrop.CurrentStack = currentStack;
                __instance.xui.dragAndDrop.PickUpType = __instance.StackLocation;
                __instance.ItemStack = itemStack.Clone();
            }

            if (___placeSound != null)
            {
                Manager.PlayXUiSound(___placeSound, 0.75f);
            }
        }
        return false;
    }

}

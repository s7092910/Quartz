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
using UnityEngine;

[HarmonyPatch(typeof(XUiC_WorkstationFuelGrid))]
public static class XUiC_WorkstationFuelGridPatch
{

    private const string TAG = "Error Reverse Patching XUiC_WorkstationFuelGrid method: ";

    [HarmonyReversePatch]
    [HarmonyPatch("onFuelItemsChanged")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void onFuelItemsChanged(XUiC_WorkstationFuelGrid instance)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "onFuelItemsChanged()");
    }
}

[HarmonyPatch(typeof(XUiC_WorkstationGrid))]
public static class XUiC_WorkstationGridPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("AddItem", new Type[] { typeof(ItemStack) })]
    public static bool AddItem(XUiC_WorkstationFuelGrid __instance, ItemStack _item, ref bool __result)
    {
        for (int i = 0; i < __instance.itemControllers.Length; i++)
        {
            XUiC_ItemStack xuiC_ItemStack = __instance.itemControllers[i];
            ItemStack itemStack = xuiC_ItemStack.ItemStack;
            if ((itemStack == null || itemStack.IsEmpty()) && xuiC_ItemStack.CanSwap(_item))
            {
                xuiC_ItemStack.ItemStack = _item;
                __result = true;
                return false;
            }
        }
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(TileEntity))]
public static class TileEntityPatch
{

    private const string TAG = "Error Reverse Patching TileEntity method: ";

    [HarmonyReversePatch]
    [HarmonyPatch("emitHeatMapEvent")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void emitHeatMapEvent(TileEntity instance, World world, EnumAIDirectorChunkEvent eventType)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "emitHeatMapEvent()");
    }
}

[HarmonyPatch(typeof(TileEntityWorkstation))]
public static class TileEntityWorkstationPatch
{

    [HarmonyPrefix]
    [HarmonyPatch("HandleFuel")]
    public static bool HandleFuel(TileEntityWorkstation __instance, World _world, float _timePassed, ref bool __result, XUiEvent_FuelStackChanged ___FuelChanged)
    {
        Block block = __instance.blockValue.Block;

        string requiredFuels = block.Properties.GetString("Workstation.RequiredFuels");

        if(string.IsNullOrEmpty(requiredFuels))
        {
            return true;
        }

        if (!__instance.isBurning)
        {
            __result = false;
            return false;
        }

        __instance.emitHeatMapEvent(_world, EnumAIDirectorChunkEvent.Campfire);
        bool flag = false;
        if (__instance.currentBurnTimeLeft > 0f || (__instance.currentBurnTimeLeft == 0f && __instance.getTotalFuelSeconds() > 0f))
        {
            __instance.currentBurnTimeLeft -= _timePassed;
            __instance.currentBurnTimeLeft = (float)Mathf.FloorToInt(__instance.currentBurnTimeLeft * 100f) / 100f;
            flag = true;
        }
        while(__instance.currentBurnTimeLeft < 0f && __instance.getTotalFuelSeconds() > 0f)
        {
            for(int i = 0; i < __instance.fuel.Length; i++)
            {
                if (__instance.fuel[i].count > 0)
                {
                    __instance.fuel[i].count--;
                    __instance.currentBurnTimeLeft += __instance.GetFuelTime(__instance.fuel[i]);
                    flag = true;
                    if (___FuelChanged != null)
                    {
                        ___FuelChanged();
                    }
                    break;
                }
            }

        }
        if (__instance.getTotalFuelSeconds() == 0f && __instance.currentBurnTimeLeft < 0f)
        {
            __instance.currentBurnTimeLeft = 0f;
            flag = true;
        }
        __result = flag;


        return false;
    }
}


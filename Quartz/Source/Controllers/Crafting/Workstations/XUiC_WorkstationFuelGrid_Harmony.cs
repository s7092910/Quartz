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
    public static bool HandleFuel(TileEntityWorkstation __instance, World _world, float _timePassed, ref bool __result, ItemStack[] ___fuel, bool ___isBurning, ref float ___currentBurnTimeLeft, XUiEvent_FuelStackChanged ___FuelChanged)
    {
        Block block = __instance.blockValue.Block;

        string requiredFuels = block.Properties.GetString("Workstation.RequiredFuels");

        if(string.IsNullOrEmpty(requiredFuels))
        {
            return true;
        }

        if (!___isBurning)
        {
            __result = false;
            return false;
        }

        TileEntityPatch.emitHeatMapEvent(__instance, _world, EnumAIDirectorChunkEvent.Campfire);
        bool flag = false;
        if (___currentBurnTimeLeft > 0f || (___currentBurnTimeLeft == 0f && getTotalFuelSeconds(___fuel) > 0f))
        {
            ___currentBurnTimeLeft -= _timePassed;
            ___currentBurnTimeLeft = (float)Mathf.FloorToInt(___currentBurnTimeLeft * 100f) / 100f;
            flag = true;
        }
        if (___currentBurnTimeLeft < 0f && getTotalFuelSeconds(___fuel) > 0f)
        {
            for(int i = 0; i < ___fuel.Length; i++)
            {
                if (___fuel[i].count > 0)
                {
                    ___fuel[i].count--;
                    ___currentBurnTimeLeft += GetFuelTime(___fuel[i]);
                    flag = true;
                    if (___FuelChanged != null)
                    {
                        ___FuelChanged();
                    }
                    break;
                }
            }

        }
        if (getTotalFuelSeconds(___fuel) == 0f && ___currentBurnTimeLeft < 0f)
        {
            ___currentBurnTimeLeft = 0f;
            flag = true;
        }
        __result = flag;


        return false;
    }

    private static float getTotalFuelSeconds(ItemStack[] fuel)
    {
        float num = 0f;
        for (int i = 0; i < fuel.Length; i++)
        {
            if (!fuel[i].IsEmpty())
            {
                num += ItemClass.GetFuelValue(fuel[i].itemValue) * fuel[i].count;
            }
        }
        return num;
    }

    private static float GetFuelTime(ItemStack _fuel)
    {
        if (_fuel.itemValue.type == 0)
        {
            return 0f;
        }
        return ItemClass.GetFuelValue(_fuel.itemValue);
    }

    private static bool ShouldCycleStacks(ItemClass[] reqItemClasses, ItemStack[] fuel)
    {
        if (!XUi.IsGameRunning())
        {
            return false;
        }

        int num = 0;
        ItemClass reqItemClass = null;
        for (int i = 0; i < fuel.Length; i++)
        {
            int currentFuelValue = 0;
            ItemStack itemStack = fuel[i];
            if (itemStack != null && !itemStack.IsEmpty())
            {
                ItemClass itemClass = itemStack.itemValue.ItemClass;
                if (itemClass != null)
                {

                    if (!itemClass.IsBlock())
                    {
                        if (itemClass.FuelValue != null)
                        {
                            currentFuelValue = itemClass.FuelValue.Value;
                        }
                    }
                    else
                    {
                        Block block = Block.list[itemClass.Id];
                        if (block != null)
                        {
                            currentFuelValue = block.FuelValue;
                        }
                    }
                }
            }

            if (itemStack == null)
            {
                continue;
            }

            if (reqItemClasses[i] != reqItemClass)
            {
                reqItemClass = reqItemClasses[i];
            }
            else if (num == 0 && currentFuelValue != 0 && i != 0)
            {
                return true;
            }

            num = currentFuelValue;
        }
        return false;
    }
}


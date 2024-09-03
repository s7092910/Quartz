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

[HarmonyPatch(typeof(XUiC_LootContainer))]
public class XUiC_LootContainerPatch
{
    private const string TAG = "XUiC_LootContainerPatch";


    //Removed as OnTileEntityChanged is called anyways when this is called

    //[HarmonyPostfix]
    //[HarmonyPatch("HandleLootSlotChangedEvent")]
    //public static void HandleLootSlotChangedEvent(XUiC_LootContainer __instance, int slotNumber, ItemStack stack)
    //{
    //    if (__instance is XUiC.XUiC_LootContainer instance)
    //    {
    //        instance.HandleSlotChangedEvent(slotNumber, stack);
    //    }
    //}

    [HarmonyPrefix]
    [HarmonyPatch("SetSlots")]
    public static void SetSlotsPrefix(XUiC_LootContainer __instance, ITileEntityLootable lootContainer, ItemStack[] stackList)
    {
        if (__instance is Quartz.XUiC_LootContainer instance)
        {
            instance.SetCurrentTileEntity(lootContainer);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetSlots")]
    public static void SetSlots(XUiC_LootContainer __instance, ITileEntityLootable lootContainer, ItemStack[] stackList)
    {
        if (__instance is Quartz.XUiC_LootContainer instance)
        {
            instance.UpdateFilterFromSearch();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnTileEntityChanged")]
    public static void OnTileEntityChanged(XUiC_LootContainer __instance, ITileEntity _te)
    {
        if (__instance is Quartz.XUiC_LootContainer instance)
        {
            instance.UpdateFilterFromSearch();
        }
    }
}

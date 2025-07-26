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

[HarmonyPatch(typeof(XUiC_VehicleContainer))]
public class XUiC_VehicleContainerPatch
{
    private const string TAG = "XUiC_VehicleContainerPatch";

    [HarmonyPostfix]
    [HarmonyPatch("HandleLootSlotChangedEvent")]
    public static void HandleLootSlotChangedEvent(XUiC_VehicleContainer __instance, int slotNumber, ItemStack stack)
    {
        if(__instance is Quartz.XUiC_VehicleContainer instance)
        {
            instance.HandleSlotChangedEvent(slotNumber, stack);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetSlots")]
    public static void SetSlotsPostfix(XUiC_VehicleContainer __instance, ItemStack[] stackList)
    {
        if (__instance is Quartz.XUiC_VehicleContainer instance)
        {
            instance.UpdateFilterFromSearch();
        }
    }
}

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

[HarmonyPatch(typeof(XUiC_BagContainer))]
public class XUiC_BagContainerPatch
{
    private const string TAG = "XUiC_BagContainerPatch";

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_BagContainer.HandleBagSlotChangedEvent))]
    public static void HandleBagSlotChangedEvent(XUiC_BagContainer __instance, int _slotNumber, ItemStack _stack)
    {
        if(__instance is Quartz.XUiC_BagContainer instance)
        {
            instance.HandleBagSlotChangedEventPost(_slotNumber, _stack);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_BagContainer.SetBag))]
    public static void SetBagsPostfix(XUiC_BagContainer __instance, Bag _bag, LootContainer _lootContainer, string _containerName, Action _onModified = null)
    {
        if (__instance is Quartz.XUiC_BagContainer instance)
        {
            instance.UpdateFilterFromSearch();
        }
    }
}

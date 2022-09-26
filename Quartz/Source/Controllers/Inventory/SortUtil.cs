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

using System.Collections.Generic;

namespace Quartz.Inventory
{
    public static class SortUtil
    {

        public static global::ItemStack[] CombineAndSortStacks(XUiC_ItemStackGrid grid, int ignoreSlots)
        {
            XUiController[] itemControllers = grid.GetItemStackControllers();

            List<global::ItemStack> itemsList = new List<global::ItemStack>();

            for (int i = ignoreSlots; i < itemControllers.Length; i++)
            {
                XUiC_ItemStack itemStack = itemControllers[i] as XUiC_ItemStack;
                if (itemStack != null)
                {
                    if (!(itemStack is ItemStack quartzItemStack) || !quartzItemStack.IsALockedSlot)
                    {
                        itemsList.Add(itemStack.ItemStack);
                    }
                }
            }

            global::ItemStack[] items = itemsList.ToArray();
            items = StackSortUtil.CombineAndSortStacks(items, 0);

            global::ItemStack[] slots = grid.GetSlots();

            int j = 0;
            for (int i = ignoreSlots; i < slots.Length; i++)
            {
                XUiC_ItemStack itemStack = itemControllers[i] as XUiC_ItemStack;
                if (itemStack != null)
                {
                    if (!(itemStack is ItemStack quartzItemStack) || !quartzItemStack.IsALockedSlot)
                    {
                        slots[i] = items[j];
                        j++;
                    }
                }
            }

            return slots;
        }
    }
}

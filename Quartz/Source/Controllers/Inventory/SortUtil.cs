using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

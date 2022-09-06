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

using QuartzOverhaul.Extensions;

namespace QuartzOverhaul.ItemStacks
{
    public class ItemStackGrid : XUiBaseController
    {
        protected XUiController[] itemControllers;

        protected ItemStack[] items;

        protected virtual ItemStackBase.StackLocationTypes StackLocation => ItemStackBase.StackLocationTypes.Backpack;

        protected override void OnInit()
        {
            itemControllers = GetChildrenWithInterface<IItemStackController>();
            IsDirty = false;
            IsDormant = true;
        }

        public XUiController[] GetItemStackControllers()
        {
            return itemControllers;
        }

        public virtual ItemStack[] GetSlots()
        {
            return getUISlots();
        }

        protected virtual ItemStack[] getUISlots()
        {
            ItemStack[] array = new ItemStack[itemControllers.Length];
            for (int i = 0; i < itemControllers.Length; i++)
            {
                array[i] = ((IItemStackController)itemControllers[i]).ItemStack.Clone();
            }

            return array;
        }

        protected virtual void SetStacks(ItemStack[] stackList)
        {
            if (stackList != null)
            {
                IItemInfoWindowController childByInterface = xui.GetChildByInterface<IItemInfoWindowController>();
                for (int i = 0; i < stackList.Length && itemControllers.Length > i && stackList.Length > i; i++)
                {
                    IItemStackController obj = (IItemStackController)itemControllers[i];
                    obj.SlotChangedEvent -= HandleSlotChangedEvent;
                    obj.ItemStack = stackList[i].Clone();
                    obj.SlotChangedEvent += HandleSlotChangedEvent;
                    obj.SlotIndex = i;
                    obj.InfoWindow = childByInterface;
                    obj.StackLocation = StackLocation;
                }
            }
        }

        public void AssembleLockSingleStack(ItemStack stack)
        {
            for (int i = 0; i < itemControllers.Length; i++)
            {
                IItemStackController xUiC_ItemStack = (IItemStackController)itemControllers[i];
                if (xUiC_ItemStack.ItemStack.itemValue.Equals(stack.itemValue))
                {
                    //TODO: Fix this
                    //xui.AssembleItem.CurrentItemStackController = xUiC_ItemStack;
                    break;
                }
            }
        }

        public virtual void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
        {
            if (items != null)
            {
                items[slotNumber] = stack.Clone();
            }

            UpdateBackend(getUISlots());
        }

        protected virtual void UpdateBackend(ItemStack[] stackList)
        {
        }

        public override void OnOpen()
        {
            if (ViewComponent != null && !ViewComponent.IsVisible)
            {
                ViewComponent.IsVisible = true;
            }

            IsDirty = true;
            IsDormant = false;
        }

        public override void OnClose()
        {
            for (int i = 0; i < itemControllers.Length; i++)
            {
                itemControllers[i].Hovered(_isOver: false);
            }

            if (ViewComponent != null && ViewComponent.IsVisible)
            {
                ViewComponent.IsVisible = false;
            }

            IsDormant = true;
        }
    }
}

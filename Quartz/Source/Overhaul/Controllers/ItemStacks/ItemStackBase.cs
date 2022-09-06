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

using System;

namespace QuartzOverhaul
{
    public class ItemStackBase : SelectableEntry, IItemStackController
    {
        public enum LockTypes
        {
            None,
            Shell,
            Crafting,
            Repairing,
            Scrapping,
            Burning
        }

        public enum StackLockTypes
        {
            None,
            Assemble,
            Quest,
            Tool,
            Hidden
        }

        public enum StackLocationTypes
        {
            Backpack,
            ToolBelt,
            LootContainer,
            Equipment,
            Creative,
            Vehicle,
            Workstation,
            Merge
        }

        protected ItemStack itemStack;
        public ItemStack ItemStack { get => itemStack; set => throw new NotImplementedException(); }

        public int SlotIndex { get; set; }

        public IItemInfoWindowController InfoWindow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public StackLocationTypes StackLocation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public XUiEvent_SlotChangedEventHandler SlotChangedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void ForceSetItemStack(ItemStack itemStack)
        {
            throw new NotImplementedException();
        }
    }

    public interface IItemStackController
    {
        ItemStack ItemStack { get; set; }

        IItemInfoWindowController InfoWindow { get; set; }

        ItemStackBase.StackLocationTypes StackLocation { get; set; }

        XUiEvent_SlotChangedEventHandler SlotChangedEvent { get; set; }

        int SlotIndex { get; set; }

        void ForceSetItemStack(ItemStack itemStack);
    }
}

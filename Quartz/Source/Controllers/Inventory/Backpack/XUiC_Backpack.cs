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

using Audio;
using Quartz.Inputs;
using Quartz.Inventory;
using System;

namespace Quartz
{
    public class XUiC_Backpack : global::XUiC_Backpack, ILockableInventory
    {
        private const string TAG = "Backpack";

        protected XUiC_BackpackWindow backpackWindow;
        protected global::XUiC_ContainerStandardControls standardControls;

        protected EntityPlayerLocal player;

        private string searchResult;

        public override void Init()
        {
            base.Init();
            backpackWindow = GetParentByType<XUiC_BackpackWindow>();
            if (backpackWindow == null)
            {
                return;
            }

            standardControls = backpackWindow.GetChildByType<global::XUiC_ContainerStandardControls>();

            XUiC_TextInput searchInput = backpackWindow.GetChildByType<XUiC_TextInput>();
            if (searchInput != null)
            {
                searchInput.OnChangeHandler += OnSearchInputChange;
                if (searchInput.UIInput != null)
                {
                    searchInput.Text = "";
                }
            }

            foreach (XUiController xUiController in GetItemStackControllers())
            {
                xUiController.OnPress += OnItemStackPress;
            }
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);

            if(player == null && XUi.IsGameRunning())
            {
                player = xui.playerUI.entityPlayer;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            QuartzInputManager.inventoryActions.Enabled = true;
        }

        public override void OnClose()
        {
            base.OnClose();
            QuartzInputManager.inventoryActions.Enabled = false;
        }

        public override void HandleSlotChangedEvent(int slotNumber, global::ItemStack stack)
        {
            if (slotNumber < itemControllers.Length)
            {
                base.HandleSlotChangedEvent(slotNumber, stack);

                XUiC_ItemStack itemStack = itemControllers[slotNumber] as XUiC_ItemStack;
                FilterFromSearch(itemStack, !string.IsNullOrEmpty(searchResult), searchResult);
            }
        }

        public override void SetStacks(global::ItemStack[] stackList)
        {
            base.SetStacks(stackList);
            FilterFromSearch(searchResult);
        }

        public int TotalLockedSlotsCount()
        {
            int count = 0;
            for (int i = 0; i < itemControllers.Length; i++)
            {
                if (itemControllers[i] is XUiC_ItemStack itemStack && itemStack.UserLockedSlot)
                {
                    count++;
                }
            }

            return count;
        }

        public int IndividualLockedSlotsCount()
        {
            int count = 0;
            for (int i = 0; i < itemControllers.Length; i++)
            {
                if (itemControllers[i] is XUiC_ItemStack itemStack && itemStack.UserLockedSlot)
                {
                    count++;
                }
            }

            return count;
        }

        public int UnlockedSlotCount()
        {
            int count = 0;
            for (int i = 0; i < itemControllers.Length; i++)
            {
                if (itemControllers[i] is XUiC_ItemStack itemStack && !itemStack.UserLockedSlot)
                {
                    count++;
                }
            }

            return count;
        }

        public bool HasLockSlotSupport()
        {
            return true;
        }

        protected void OnSearchInputChange(XUiController sender, string text, bool changeFromCode)
        {
            searchResult = text;
            FilterFromSearch(text);
        }

        protected virtual void OnItemStackPress(XUiController sender, int mouseButton)
        {
            if (sender is global::XUiC_ItemStack itemStack
                && (QuartzInputManager.inventoryActions.LockSlot.IsPressed))
            {
                int index = Array.IndexOf(itemControllers, itemStack);
                itemStack.UserLockedSlot = !itemStack.UserLockedSlot;
                Manager.PlayXUiSound(xui.uiClickSound, 0.75f);
                backpackWindow.UpdateLockedSlots(standardControls);
            }
        }

        private void FilterFromSearch(string search)
        {
            bool activeSearch = !string.IsNullOrEmpty(search);
            foreach (var itemController in itemControllers)
            {
                XUiC_ItemStack itemStack = itemController as XUiC_ItemStack;
                FilterFromSearch(itemStack, activeSearch, search);
            }
        }

        private void FilterFromSearch(XUiC_ItemStack itemStack, bool activeSearch, string search)
        {
            if (itemStack == null)
            {
                return;
            }
            if (activeSearch)
            {
                itemStack.MatchesSearch = SearchUtil.MatchesSearch(itemStack.ItemStack, search);
            }
            else
            {
                itemStack.MatchesSearch = false;
            }
            itemStack.IsSearchActive = activeSearch;
        }
    }
}

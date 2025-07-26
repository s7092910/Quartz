/*Copyright 2025 Christopher Beda

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
    public class XUiC_LootContainer : global::XUiC_LootContainer, ILockableInventory
    {

        private const string TAG = "LootContainer";

        protected global::XUiC_ContainerStandardControls standardControls;
        protected XUiC_LootWindow lootWindow;

        protected string searchResult;

        public override void Init()
        {
            base.Init();
            lootWindow = GetParentByType<XUiC_LootWindow>();
            if (lootWindow == null)
            {
                return;
            }

            standardControls = lootWindow.GetChildByType<global::XUiC_ContainerStandardControls>();

            XUiC_TextInput searchInput = lootWindow.GetChildByType<XUiC_TextInput>();
            if (searchInput != null)
            {
                searchInput.OnChangeHandler += OnSearchInputChange;
                if (searchInput.UIInput != null)
                {
                    searchInput.Text = "";
                }
            }

            XUiController controller = standardControls.GetChildById("btnToggleLockMode");

            if (controller != null) 
            { 
                foreach (XUiController xUiController in GetItemStackControllers())
                {
                    xUiController.OnPress += OnItemStackPress;
                }
            }
        }

        public override void UpdateInput()
        {
            base.UpdateInput();
            PlayerActionsLocal playerInput = xui.playerUI.playerInput;
            if (lootWindow.UserLockMode && (playerInput.GUIActions.Cancel.WasPressed || playerInput.PermanentActions.Cancel.WasPressed))
            {
                lootWindow.UserLockMode = false;
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

        protected void OnSearchInputChange(XUiController sender, string text, bool changeFromCode)
        {
            searchResult = text;
            FilterFromSearch(text);
        }

        public override void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
        {
            if (slotNumber < itemControllers.Length)
            {
                XUiC_ItemStack itemStack = itemControllers[slotNumber] as XUiC_ItemStack;
                FilterFromSearch(itemStack, !string.IsNullOrEmpty(searchResult), searchResult);
            }
        }

        public void UpdateFilterFromSearch()
        {
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

        protected virtual void OnItemStackPress(XUiController sender, int mouseButton)
        {
            if (localTileEntity.HasSlotLocksSupport && sender is global::XUiC_ItemStack itemStack
                && (QuartzInputManager.inventoryActions.LockSlot.IsPressed || lootWindow.UserLockMode))
            {
                int index = Array.IndexOf(itemControllers, itemStack);
                itemStack.UserLockedSlot = !itemStack.UserLockedSlot;
                Manager.PlayXUiSound(xui.uiClickSound, 0.75f);
                lootWindow.UpdateLockedSlots(standardControls);
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

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
    public class XUiC_VehicleContainer : global::XUiC_VehicleContainer, ILockableInventory
    {
        private const string TAG = "VehicleContainer";

        private string searchResult;

        public override void Init()
        {
            base.Init();

            XUiC_TextInput searchInput = GetChildByType<XUiC_TextInput>();
            if (searchInput != null)
            {
                searchInput.OnChangeHandler += OnSearchInputChange;
                if (searchInput.UIInput != null)
                {
                    searchInput.Text = "";
                }
            }

            standardControls = GetChildByType<global::XUiC_ContainerStandardControls>();

            if (standardControls.GetChildById("btnToggleLockMode") != null)
            {
                foreach (XUiController xUiController in GetItemStackControllers())
                {
                    xUiController.OnPress += OnItemStackPress;
                }
            }
        }

        public override void Update(float _dt)
        {
            if (GameManager.Instance == null && GameManager.Instance.World == null)
            {
                return;
            }

            if (IsDirty)
            {
                hasStorage = xui.vehicle.GetVehicle().HasStorage();
                ViewComponent.IsVisible = hasStorage;
                RefreshBindings(false);
                IsDirty = false;
            }

            XUiControllerPatch.Update(this, _dt);

            if (!windowGroup.isShowing)
            {
                return;
            }

            if (!xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
            {
                wasReleased = true;
            }

            if (wasReleased)
            {
                if (xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
                {
                    activeKeyDown = true;
                }

                if (xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDown && !xui.playerUI.windowManager.IsInputActive())
                {
                    activeKeyDown = false;
                    OnClose();
                    xui.playerUI.windowManager.CloseAllOpenWindows();
                }
            }

            if (!isClosing && ViewComponent != null && ViewComponent.IsVisible && items != null && !xui.playerUI.windowManager.IsInputActive()
                && (xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
            {
                if (standardControls is XUiC_ContainerStandardControls controls)
                {
                    controls.MoveAllButLocked();
                }
                else
                {
                    standardControls.MoveAll();
                }
            }
        }

        public override void UpdateInput()
        {
            base.UpdateInput();
            PlayerActionsLocal playerInput = xui.playerUI.playerInput;
            if (UserLockMode && (playerInput.GUIActions.Cancel.WasPressed || playerInput.PermanentActions.Cancel.WasPressed))
            {
                UserLockMode = false;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            standardControls.SortPressed = OnSortPressed;
            QuartzInputManager.inventoryActions.Enabled = true;
        }

        public override void OnClose()
        {
            base.OnClose();
            QuartzInputManager.inventoryActions.Enabled = false;
            UserLockMode = false;
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
            return currentVehicleEntity.bag.LockedSlots != null;
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

        protected void OnSearchInputChange(XUiController sender, string text, bool changeFromCode)
        {
            searchResult = text;
            FilterFromSearch(text);
        }


        protected virtual void OnSortPressed(PackedBoolArray ignoreSlots)
        {
            base.btnSort_OnPress(ignoreSlots);
        }

        protected void OnItemStackPress(XUiController sender, int mouseButton)
        {
            if (sender is XUiC_ItemStack itemStack
                && (QuartzInputManager.inventoryActions.LockSlot.IsPressed || UserLockMode))
            {
                int index = Array.IndexOf(itemControllers, itemStack);
                itemStack.UserLockedSlot = !itemStack.UserLockedSlot;
                Manager.PlayXUiSound(xui.uiClickSound, 0.75f);
                base.UpdateLockedSlots(standardControls);
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

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
using HarmonyLib;
using Platform.Local;
using Quartz.Inputs;
using Quartz.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Quartz
{
    public class XUiC_VehicleContainer : global::XUiC_VehicleContainer, ILockableInventory
	{
		private const string TAG = "VehicleContainer";
        private const string lockedSlotsCvarName = "$varQuartzVehicleLockedSlots";

        protected global::XUiC_ContainerStandardControls standardControls;
        protected XUiC_ComboBoxInt comboBox;

        protected EntityVehicle vehicle;

		protected int ignoredLockedSlots;

		private string searchResult;

        public bool IsIndividualSlotLockingAllowed { get; set; }

        public override void Init()
		{
			base.Init();

			comboBox = GetChildByType<XUiC_ComboBoxInt>();
			if (comboBox != null)
			{
				comboBox.OnValueChanged += OnLockedSlotsChange;
			}

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
            standardControls.LockModeToggled = OnLockModeToggled;

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
				ViewComponent.IsVisible = xui.vehicle.GetVehicle().HasStorage();
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
            vehicle = null;
        }

        public virtual void SetCurrentVehicle()
        {
            vehicle = xui.vehicle;
            LoadLockedSlots();
        }

        public void OnLockModeToggled()
        {
            IsIndividualSlotLockingAllowed = !IsIndividualSlotLockingAllowed;
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
                if (i >= ignoredLockedSlots && itemControllers[i] is XUiC_ItemStack itemStack && itemStack.UserLockedSlot)
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
                if (i >= ignoredLockedSlots && itemControllers[i] is XUiC_ItemStack itemStack && !itemStack.UserLockedSlot)
                {
                    count++;
                }
            }

            return count;
        }

        public bool[] GetLockSlots()
        {
            bool[] bools = new bool[itemControllers.Length];
            for (int i = 0; i < itemControllers.Length; i++)
            {
                bools[i] = itemControllers[i].UserLockedSlot;
            }

            return bools;
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

		protected virtual void OnLockedSlotsChange(XUiController sender, long value, long newValue)
		{

            for (int i = 0; i < itemControllers.Length; i++)
            {
                global::XUiC_ItemStack itemStack = itemControllers[i];
                if (value > i && i >= newValue)
                {
                    itemStack.UserLockedSlot = false;
                }

                if (newValue > i && i >= value)
                {
                    itemStack.UserLockedSlot = true;
                }
            }

			ignoredLockedSlots = (int)newValue;

            SaveLockedSlots();
        }

        protected virtual void OnSortPressed(bool[] ignoreSlots)
        {
            if (xui.vehicle.GetVehicle() == null)
			{
                return;
            }
            base.btnSort_OnPress(GetLockSlots());
        }

        protected void OnItemStackPress(XUiController sender, int mouseButton)
        {
            if (sender is XUiC_ItemStack itemStack
                && (QuartzInputManager.inventoryActions.LockSlot.IsPressed || IsIndividualSlotLockingAllowed))
			{
                int index = Array.IndexOf(itemControllers, itemStack);
                if(index >= ignoredLockedSlots)
                {
                    itemStack.UserLockedSlot = !itemStack.UserLockedSlot;
                    Manager.PlayButtonClick();
                    SaveLockedSlots();
                }
            }
        }

        protected void UpdateIgnoredLockedSlots()
        {
            if (standardControls == null)
            {
                return;
            }

            if (comboBox != null)
            {
                comboBox.Value = ignoredLockedSlots;
            }
        }

        protected virtual void SaveLockedSlots()
        {
            if (vehicle == null)
            {
                return;
            }

			BitArray bitArray = new BitArray(itemControllers.Length);
            for (int i = 0; i < bitArray.Length; i++)
            {
                global::XUiC_ItemStack itemStack = itemControllers[i];
                if (itemStack.UserLockedSlot)
                {
                    bitArray.Set(i, true);
                }
            }

            SaveLockedSlotsData(bitArray);
        }

        protected virtual void LoadLockedSlots()
        {
            if (vehicle == null)
            {
                return;
            }

            BitArray bitArray = LoadLockedSlotsData();

            UpdateIgnoredLockedSlots();

            for (int i = 0; i < itemControllers.Length; i++)
            {
                global::XUiC_ItemStack itemStack = itemControllers[i];
                itemStack.UserLockedSlot = bitArray.Get(i);
            }
        }

        private void SaveLockedSlotsData(BitArray bitArray)
        {
            List<PlatformUserIdentifierAbs> userIds = vehicle.GetVehicle().AllowedUsers;
            byte[] bytes = new byte[(bitArray.Length - 1) / 8 + 1];
            bitArray.CopyTo(bytes, 0);

            string newUserId = lockedSlotsCvarName + "," + ignoredLockedSlots + "," + Convert.ToBase64String(bytes);

            for (int i = 0; i < userIds.Count; i++)
            {
                if (userIds[i] is UserIdentifierLocal userId)
                {
                    string[] idStrings = userId.PlayerName.Split(',');
                    if (idStrings[0] == lockedSlotsCvarName)
                    {
                        userIds[i] = new UserIdentifierLocal(newUserId);
                        return;
                    }
                }
            }

            userIds.Insert(0, new UserIdentifierLocal(newUserId));
        }

        private BitArray LoadLockedSlotsData()
        {
            ignoredLockedSlots = 0;
            foreach (PlatformUserIdentifierAbs userId in vehicle.GetVehicle().AllowedUsers)
			{
                if (userId is UserIdentifierLocal user)
                {
                    string[] idStrings = user.PlayerName.Split(',');
                    if (idStrings[0] == lockedSlotsCvarName && idStrings.Length == 3)
                    {
                        ignoredLockedSlots = int.Parse(idStrings[1]);
						byte[] bytes = Convert.FromBase64String(idStrings[2]);
						return new BitArray(bytes);
                    }
                }
            }

            return new BitArray(itemControllers.Length);
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

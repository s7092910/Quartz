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
	public class VehicleContainer : global::XUiC_VehicleContainer
	{
		private const string TAG = "VehicleContainer";
        private const string lockedSlotsCvarName = "$varQuartzVehicleLockedSlots";

        private XUiC_ContainerStandardControls controls;

		private EntityVehicle vehicle;

		private int ignoredSlots;
		private string searchResult;

		private Traverse isClosingTraverse;
		private Traverse wasReleasedTraverse;
		private Traverse activeKeyDownTraverse;

		public override void Init()
		{
			base.Init();

			XUiC_ComboBoxInt comboBox = GetChildByType<XUiC_ComboBoxInt>();
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

			controls = GetChildByType<XUiC_ContainerStandardControls>();

            if (controls != null && controls is ContainerStandardControls)
            {
                controls.OnSortPressed = OnSortPressed;
                foreach (XUiController itemStackController in GetItemStackControllers())
				{
                    itemStackController.OnPress += OnItemStackPress;
                }
            }

            isClosingTraverse = Traverse.Create(this).Field("isClosing");
            wasReleasedTraverse = Traverse.Create(this).Field("wasReleased");
            activeKeyDownTraverse = Traverse.Create(this).Field("activeKeyDown");
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
				wasReleasedTraverse.SetValue(true);
			}

			if (wasReleasedTraverse.GetValue<bool>())
			{
				if (xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
				{
					activeKeyDownTraverse.SetValue(true);
				}

				if (xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDownTraverse.GetValue<bool>() && !xui.playerUI.windowManager.IsInputActive())
				{
					activeKeyDownTraverse.SetValue(false);
					OnClose();
					xui.playerUI.windowManager.CloseAllOpenWindows();
				}
			}

			if (!isClosingTraverse.GetValue<bool>() && ViewComponent != null && ViewComponent.IsVisible && items != null && !xui.playerUI.windowManager.IsInputActive()
				&& (xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
			{
                if (controls is ContainerStandardControls quartzControls)
                {
                    quartzControls.MoveAllButLocked();
                }
                else
                {
                    controls.MoveAll();
                }
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
            vehicle = null;
        }

        public void SetCurrentVehicle()
        {
            vehicle = xui.vehicle;
            LoadLockedSlots();
        }

        protected void OnSearchInputChange(XUiController sender, string text, bool changeFromCode)
		{
			searchResult = text;
			FilterFromSearch(text);
		}

		protected void OnLockedSlotsChange(XUiController sender, long value, long newValue)
		{

            for (int i = 0; i < itemControllers.Length; i++)
            {
                ItemStack itemStack = itemControllers[i] as ItemStack;
                if (itemStack != null)
                {
                    if (value > i && i >= newValue)
                    {
                        itemStack.IsALockedSlot = false;
                    }

                    if (newValue > i && i >= value)
                    {
                        itemStack.IsALockedSlot = true;
					}
				}
			}

			ignoredSlots = (int)newValue;

            SaveLockedSlots();
        }

		public override void HandleSlotChangedEvent(int slotNumber, global::ItemStack stack)
		{
			if (slotNumber < itemControllers.Length)
			{
				ItemStack itemStack = itemControllers[slotNumber] as ItemStack;
				FilterFromSearch(itemStack, !string.IsNullOrEmpty(searchResult), searchResult);
			}
		}

		public void UpdateFilterFromSearch()
		{
			FilterFromSearch(searchResult);
		}

        protected void OnSortPressed(int ignoreSlots)
        {
            if (xui.vehicle.GetVehicle() == null)
			{
                return;
            }
            xui.vehicle.bag.SetSlots(SortUtil.CombineAndSortStacks(this, ignoreSlots));
        }

        protected void OnItemStackPress(XUiController sender, int mouseButton)
        {
            if (sender is ItemStack itemStack && QuartzInputManager.inventoryActions.LockSlot.IsPressed)
			{
                int index = Array.IndexOf(itemControllers, itemStack);
                if(index >= ignoredSlots)
                {
                    itemStack.IsALockedSlot = !itemStack.IsALockedSlot;
                    Manager.PlayButtonClick();
                    SaveLockedSlots();
                }
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
                ItemStack itemStack = itemControllers[i] as ItemStack;
                if (itemStack != null && itemStack.IsALockedSlot)
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

            for (int i = 0; i < itemControllers.Length; i++)
            {
                ItemStack itemStack = itemControllers[i] as ItemStack;
                if (itemStack != null)
                {
                    itemStack.IsALockedSlot = bitArray.Get(i);
                }
            }
        }

        private void SaveLockedSlotsData(BitArray bitArray)
        {
            List<PlatformUserIdentifierAbs> userIds = vehicle.GetVehicle().AllowedUsers;
            byte[] bytes = new byte[(bitArray.Length - 1) / 8 + 1];
            bitArray.CopyTo(bytes, 0);

            string newUserId = lockedSlotsCvarName + "," + ignoredSlots + "," + Convert.ToBase64String(bytes);

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
            foreach (PlatformUserIdentifierAbs userId in vehicle.GetVehicle().AllowedUsers)
			{
                if (userId is UserIdentifierLocal user)
                {
                    string[] idStrings = user.PlayerName.Split(',');
                    if (idStrings[0] == lockedSlotsCvarName && idStrings.Length == 3)
                    {
                        ignoredSlots = int.Parse(idStrings[1]);
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
				ItemStack itemStack = itemController as ItemStack;
				FilterFromSearch(itemStack, activeSearch, search);
			}
		}

		private void FilterFromSearch(ItemStack itemStack, bool activeSearch, string search)
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

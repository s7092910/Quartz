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
using System.Collections.Generic;
using System.Xml.Linq;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace Quartz
{
	public class VehicleContainer : global::XUiC_VehicleContainer
	{
		private const string TAG = "VehicleContainer";
        private const string lockedSlotsCvarName = "$varQuartzVehicleLockedSlots";

		private XUiC_VehicleStorageWindowGroup parent;
        private XUiC_ContainerStandardControls controls;
		
		private EntityVehicle vehicle;

		private Dictionary<string, float> cvars;

		private string searchResult;

		private Traverse isClosingTraverse;
		private Traverse wasReleasedTraverse;
		private Traverse activeKeyDownTraverse;

		public override void Init()
		{
			base.Init();

			cvars = new Dictionary<string, float>();

			parent = GetParentByType<XUiC_VehicleStorageWindowGroup>();

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
				controls.MoveAll();
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
            vehicle = parent.CurrentVehicleEntity;
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

            cvars[lockedSlotsCvarName] = newValue;

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
                itemStack.IsALockedSlot = !itemStack.IsALockedSlot;
                Manager.PlayButtonClick();
                SaveLockedSlots();
            }
        }

        protected virtual void SaveLockedSlots()
        {
            if (vehicle == null)
            {
                return;
            }

            int saveArrayCount = itemControllers.Length / 20;

            if (itemControllers.Length % 20 != 0)
            {
                saveArrayCount++;
            }

            for (int i = 0; i < saveArrayCount; i++)
            {
                int flag = 0;
                int indexOffset = i * 20;

                for (int j = 0; j < 20 && (j + indexOffset) < itemControllers.Length; j++)
                {
                    ItemStack itemStack = itemControllers[j + indexOffset] as ItemStack;
                    if (itemStack != null && itemStack.IsALockedSlot)
                    {
                        flag |= 1 << j;
                    }
                }

				cvars[lockedSlotsCvarName + i] = flag;
            }

            SaveVehicleCVars();
        }

        protected virtual void LoadLockedSlots()
        {
            if (vehicle == null)
            {
                return;
            }

            LoadVehicleCvars();

            int saveArrayCount = itemControllers.Length / 20;

            if (itemControllers.Length % 20 != 0)
            {
                saveArrayCount++;
            }

            for (int i = 0; i < saveArrayCount; i++)
            {
                int flag = (int)cvars[lockedSlotsCvarName + i];
                int indexOffset = i * 20;

                for (int j = 0; j < 20 && (j + indexOffset) < itemControllers.Length; j++)
                {
                    ItemStack itemStack = itemControllers[j + indexOffset] as ItemStack;
                    if (itemStack != null)
                    {
                        itemStack.IsALockedSlot = (flag & (1 << j)) != 0;
                    }
                }
            }
        }

		private void LoadVehicleCvars()
		{
            int cVarCount = itemControllers.Length / 20;

            if (itemControllers.Length % 20 != 0)
            {
                cVarCount++;
            }

            cvars.Clear();

			List<PlatformUserIdentifierAbs> userIds = vehicle.GetVehicle().AllowedUsers;
			for(int i = 0; i < cVarCount + 1 && i < userIds.Count; i++)
			{
				if (userIds[i] is UserIdentifierLocal userId)
				{
					string[] idStrings = userId.PlayerName.Split(',');
					if (idStrings[0] == lockedSlotsCvarName || idStrings[0].Substring(0, lockedSlotsCvarName.Length) == lockedSlotsCvarName)
					{
						cvars.Add(idStrings[0],float.Parse(idStrings[1]));
					}
				}
			}

			if(cvars.Count <= 0)
			{
				for (int i = cVarCount - 1; i >= 0; i--)
				{
					userIds.Insert(0, new UserIdentifierLocal(lockedSlotsCvarName + i + "," + 0));
					cvars.Add(lockedSlotsCvarName + i, 0);
                }

                userIds.Insert(0, new UserIdentifierLocal(lockedSlotsCvarName + "," + 0));
                cvars.Add(lockedSlotsCvarName, 0);
            }
		}

		private void SaveVehicleCVars()
		{
            List<PlatformUserIdentifierAbs> userIds = vehicle.GetVehicle().AllowedUsers;
            for (int i = 0; i < userIds.Count; i++)
            {
                if (userIds[i] is UserIdentifierLocal userId)
                {
                    string[] idStrings = userId.PlayerName.Split(',');
                    if (idStrings[0] == lockedSlotsCvarName || idStrings[0].Substring(0, lockedSlotsCvarName.Length) == lockedSlotsCvarName)
                    {
                        userIds[i] = new UserIdentifierLocal(idStrings[0] + "," + cvars[idStrings[0]]);
                    }
                }
            }
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

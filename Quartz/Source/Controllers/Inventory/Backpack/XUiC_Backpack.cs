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

		private const string lockedSlotsCvarName = "$varQuartzBackpackLockedSlots";

        protected XUiC_ContainerStandardControls standardControls;
        protected XUiC_ComboBoxInt comboBox;

        protected EntityPlayer player;

        protected int ignoredLockedSlots;

        private string searchResult;

        public override void Init()
		{
			base.Init();
			XUiController parent = GetParentByType<XUiC_BackpackWindow>();
			if (parent == null)
			{
				return;
			}

			standardControls = parent.GetChildByType<XUiC_ContainerStandardControls>();

			comboBox = standardControls.GetChildByType<XUiC_ComboBoxInt>();
			if (comboBox != null)
			{
				comboBox.OnValueChanged += OnLockedSlotsChange;
			}

			XUiC_TextInput searchInput = parent.GetChildByType<XUiC_TextInput>();
			if (searchInput != null)
			{
				searchInput.OnChangeHandler += OnSearchInputChange;
				if (searchInput.UIInput != null)
				{
					searchInput.Text = "";
				}
			}

            if(standardControls != null && standardControls is XUiC_ContainerStandardControls)
            {
                foreach (XUiController xUiController in GetItemStackControllers())
                {
                    xUiController.OnPress += OnItemStackPress;
                }
            }
		}

        public override void Update(float _dt)
		{
			base.Update(_dt);

			if(player == null && XUi.IsGameRunning())
			{
				player = xui.playerUI.entityPlayer;

				LoadLockedSlots();

                if(standardControls != null && comboBox != null)
                {
                    comboBox.Value = ignoredLockedSlots;
                    standardControls.ChangeLockedSlots(ignoredLockedSlots);
                }

                if (standardControls is XUiC_ContainerStandardControls controls)
                {
                    standardControls.OnSortPressed = OnSortPressed;
                    controls.ChangeLockedSlots(ignoredLockedSlots);
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

        public int TotalLockedSlotsCount()
        {
            int count = 0;
            for (int i = 0; i < itemControllers.Length; i++)
            {
                if (itemControllers[i] is XUiC_ItemStack itemStack && itemStack.IsALockedSlot)
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
                if (i >= ignoredLockedSlots && itemControllers[i] is XUiC_ItemStack itemStack && itemStack.IsALockedSlot)
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
                if (i >= ignoredLockedSlots && itemControllers[i] is XUiC_ItemStack itemStack && !itemStack.IsALockedSlot)
                {
                    count++;
                }
            }

            return count;
        }

        protected override void SetStacks(global::ItemStack[] stackList)
        {
            base.SetStacks(stackList);
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
                XUiC_ItemStack itemStack = itemControllers[i] as XUiC_ItemStack;
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

            player.SetCVar(lockedSlotsCvarName, newValue);
            ignoredLockedSlots = (int)newValue;

            SaveLockedSlots();
        }

        protected virtual void OnSortPressed(int ignoreSlots)
        {
            global::ItemStack[] slots = SortUtil.CombineAndSortStacks(this, ignoreSlots);
            xui.PlayerInventory.Backpack.SetSlots(slots);
        }

        protected virtual void OnItemStackPress(XUiController sender, int mouseButton)
        {
            if (sender is XUiC_ItemStack itemStack && standardControls is XUiC_ContainerStandardControls controls 
                && (QuartzInputManager.inventoryActions.LockSlot.IsPressed || controls.IsIndividualSlotLockingAllowed()))
            {
                int index = Array.IndexOf(itemControllers, itemStack);
                if (index >= ignoredLockedSlots)
                {
                    itemStack.IsALockedSlot = !itemStack.IsALockedSlot;
                    Manager.PlayButtonClick();
                    SaveLockedSlots();
                    controls.RefreshBindings();
                }
            }
        }

        protected virtual void SaveLockedSlots()
        {
            if (player == null)
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
                    XUiC_ItemStack itemStack = itemControllers[j + indexOffset] as XUiC_ItemStack;
                    if (itemStack != null && itemStack.IsALockedSlot)
                    {
                        flag |= 1 << j;
                    }
                }

                player.SetCVar(lockedSlotsCvarName + i, flag);
            }
        }

        protected virtual void LoadLockedSlots()
        {
            if (player == null)
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
                int flag = (int)player.GetCVar(lockedSlotsCvarName + i);
                int indexOffset = i * 20;

                for (int j = 0; j < 20 && (j + indexOffset) < itemControllers.Length; j++)
                {
                    XUiC_ItemStack itemStack = itemControllers[j + indexOffset] as XUiC_ItemStack;
                    if (itemStack != null && (flag & (1 << j)) != 0)
                    {
                        itemStack.IsALockedSlot = true;
                    }
                }
            }

            ignoredLockedSlots = (int)player.GetCVar(lockedSlotsCvarName);
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

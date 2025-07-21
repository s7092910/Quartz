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
using UnityEngine;

namespace Quartz
{
	public class XUiC_Backpack : global::XUiC_Backpack
	{
		private const string TAG = "Backpack";

		private const string lockedSlotsCvarName = "$varQuartzBackpackLockedSlots";

        protected XUiC_BackpackWindow backpackWindow;
        protected global::XUiC_ContainerStandardControls standardControls;
        protected XUiC_ComboBoxInt comboBox;

        protected EntityPlayerLocal player;

        protected int ignoredLockedSlots;

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

			comboBox = standardControls.GetChildByType<XUiC_ComboBoxInt>();
			if (comboBox != null)
			{
				comboBox.OnValueChanged += OnLockedSlotsChange;
			}

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

				LoadLockedSlots();

                if(standardControls != null && comboBox != null)
                {
                    comboBox.Value = ignoredLockedSlots;
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

        public override void SetStacks(global::ItemStack[] stackList)
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

            player.SetCVar(lockedSlotsCvarName, newValue);
            ignoredLockedSlots = (int)newValue;
            backpackWindow.UpdateLockedSlots(standardControls);
        }

        protected virtual void OnItemStackPress(XUiController sender, int mouseButton)
        {
            if (sender is global::XUiC_ItemStack itemStack
                && (QuartzInputManager.inventoryActions.LockSlot.IsPressed))
            {
                int index = Array.IndexOf(itemControllers, itemStack);
                if (index >= ignoredLockedSlots)
                {
                    itemStack.UserLockedSlot = !itemStack.UserLockedSlot;
                    Manager.PlayXUiSound(xui.uiClickSound, 0.75f);
                    backpackWindow.UpdateLockedSlots(standardControls);
                }
            }
        }

        protected virtual void LoadLockedSlots()
        {
            if (player == null)
            {
                return;
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

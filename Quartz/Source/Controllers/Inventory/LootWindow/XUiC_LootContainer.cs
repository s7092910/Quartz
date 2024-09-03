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
using Platform.Local;
using Quartz.Inputs;
using Quartz.Inventory;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Quartz
{
	public class XUiC_LootContainer : global::XUiC_LootContainer, ILockableInventory
	{

        private const string TAG = "LootContainer";
        private const string lockedSlotsCvarName = "$varQuartzLootContainerLockedSlots";

        protected global::XUiC_ContainerStandardControls standardControls;
        protected XUiC_LootWindow lootWindow;
        protected XUiC_ComboBoxInt comboBox;
        protected XUiV_Button toggleLockModeButton;

        protected int ignoredLockedSlots;

        protected string searchResult;

        protected bool userLockMode;

        public bool IsIndividualSlotLockingAllowed
        {
            get
            {
                return userLockMode;
            }
            set
            {
                if (value == userLockMode)
                {
                    return;
                }
                if (standardControls != null)
                {
                    standardControls.LockModeChanged(value);
                }
                userLockMode = value;
                WindowGroup.isEscClosable = !userLockMode;
                xui.playerUI.windowManager.GetModalWindow().isEscClosable = !userLockMode;
                RefreshBindings();
                if(lootWindow != null)
                {
                    lootWindow.RefreshBindings();
                }
            }
        }

        public override void Init()
		{
			base.Init();
			lootWindow = GetParentByType<XUiC_LootWindow>();
			if (lootWindow == null)
			{
				return;
			}

            standardControls = lootWindow.GetChildByType<global::XUiC_ContainerStandardControls>();
            standardControls.LockModeToggled = OnLockModeToggled;

            comboBox = standardControls.GetChildByType<XUiC_ComboBoxInt>();
            if (comboBox != null)
            {
                comboBox.OnValueChanged += OnLockedSlotsChange;
            }

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
                toggleLockModeButton = controller.ViewComponent as XUiV_Button;

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
            if (IsIndividualSlotLockingAllowed && (playerInput.GUIActions.Cancel.WasPressed || playerInput.PermanentActions.Cancel.WasPressed))
            {
                IsIndividualSlotLockingAllowed = false;
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
            IsIndividualSlotLockingAllowed = false;
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "userlockmode":
                    value = userLockMode.ToString();
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        public virtual void SetCurrentTileEntity(ITileEntityLootable container)
        {
            LoadLockedSlots();
        }

        protected void OnSearchInputChange(XUiController sender, string text, bool changeFromCode)
        {
            searchResult = text;
            FilterFromSearch(text);
        }

        protected virtual void OnLockedSlotsChange(XUiController sender, long value, long newValue)
        {
            ILockable lootContainer;
            if (!localTileEntity.TryGetSelfOrFeature(out lootContainer) || !localTileEntity.bPlayerStorage)
            {
                return;
            }

            for (int i = 0; i < itemControllers.Length; i++)
            {
                XUiC_ItemStack itemStack = itemControllers[i] as XUiC_ItemStack;
                if (itemStack != null)
                {
                    if (value > i && i >= newValue)
                    {
                        itemStack.UserLockedSlot = false;
                    }

                    if (newValue > i && i >= value)
                    {
                        itemStack.UserLockedSlot = true;
                    }
                }
            }

            ignoredLockedSlots = (int)newValue;
            SaveLockedSlots();
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
            for (int i = 0;i < itemControllers.Length;i++)
            {
                bools[i] = itemControllers[i].UserLockedSlot;
            }

            return bools;
        }

        protected virtual void OnSortPressed(bool[] _ignoredSlots)
        {
            ItemStack[] array = StackSortUtil.CombineAndSortStacks(localTileEntity.items, 0, GetLockSlots());
            for (int i = 0; i < array.Length; i++)
            {
                localTileEntity.UpdateSlot(i, array[i]);
            }
        }

        protected virtual void OnItemStackPress(XUiController sender, int mouseButton)
        {
            if (localTileEntity.TryGetSelfOrFeature<ILockable>(out var lootContainer) && sender is global::XUiC_ItemStack itemStack
                && (QuartzInputManager.inventoryActions.LockSlot.IsPressed || IsIndividualSlotLockingAllowed))
            {
                int index = Array.IndexOf(itemControllers, itemStack);
                if (index >= ignoredLockedSlots)
                {
                    itemStack.UserLockedSlot = !itemStack.UserLockedSlot;
                    Manager.PlayXUiSound(xui.uiClickSound, 0.75f);
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
                comboBox.Enabled = localTileEntity.TryGetSelfOrFeature<ILockable>(out var lootContainer);
            }

            if(toggleLockModeButton != null)
            {
                toggleLockModeButton.Enabled = localTileEntity.TryGetSelfOrFeature<ILockable>(out var lootContainer);
            }
        }

        protected virtual void SaveLockedSlots()
        {
            if (localTileEntity == null || !localTileEntity.bPlayerStorage)
            {
                return;
            }

            BitArray bitArray = new BitArray(itemControllers.Length);
            for (int i = 0; i < bitArray.Length; i++)
            {
                XUiC_ItemStack itemStack = itemControllers[i] as XUiC_ItemStack;
                if (itemStack != null && itemStack.UserLockedSlot)
                {
                    bitArray.Set(i, true);
                }
            }

            SaveLockedSlotsData(bitArray);
        }

        protected virtual void LoadLockedSlots()
        {
            if (localTileEntity == null)
            {
                return;
            }

            BitArray bitArray = LoadLockedSlotsData();
            UpdateIgnoredLockedSlots();

            for (int i = 0; i < itemControllers.Length; i++)
            {
                XUiC_ItemStack itemStack = itemControllers[i] as XUiC_ItemStack;
                if (itemStack != null)
                {
                    itemStack.UserLockedSlot = bitArray.Get(i);
                }
            }
        }

        private void SaveLockedSlotsData(BitArray bitArray)
        {
            ILockable lockableContainer;
            if (!localTileEntity.TryGetSelfOrFeature<ILockable>(out lockableContainer) || !localTileEntity.bPlayerStorage)
            {
                return;
            }

            List<PlatformUserIdentifierAbs> userIds = lockableContainer.GetUsers();
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
            ILockable lockableContainer;
            ignoredLockedSlots = 0;
            if (!localTileEntity.TryGetSelfOrFeature<ILockable>(out lockableContainer))
            {
                return new BitArray(itemControllers.Length);
            }

            foreach (PlatformUserIdentifierAbs userId in lockableContainer.GetUsers())
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

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

using System.Collections;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace Quartz
{
	public class Backpack : global::XUiC_Backpack
	{
		private const string TAG = "Backpack";

		private const string lockedSlotsCvarName = "$varQuartzBackpackLockedSlots";

        private XUiC_ComboBoxInt comboBox;
        private XUiC_ContainerStandardControls standardControls;

		private EntityPlayer player;

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
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);

			if(player == null && XUi.IsGameRunning())
			{
				player = xui.playerUI.entityPlayer;

				LoadLockedSlots();

                if(standardControls != null && !(standardControls is SmartLockContainerStandardControls))
                {
                    int lockedSlotsCount = (int)player.GetCVar(lockedSlotsCvarName);
                    comboBox.Value = lockedSlotsCount;
                    standardControls.ChangeLockedSlots(lockedSlotsCount);
                }
			}
		}

		public override void HandleSlotChangedEvent(int slotNumber, global::ItemStack stack)
		{
			if (slotNumber < itemControllers.Length)
			{
				base.HandleSlotChangedEvent(slotNumber, stack);

				ItemStack itemStack = itemControllers[slotNumber] as ItemStack;
				FilterFromSearch(itemStack, !string.IsNullOrEmpty(searchResult), searchResult);
			}
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

            player.SetCVar(lockedSlotsCvarName, newValue);

            SaveLockedSlots();
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

                for (int j = 0; j < 20; j++)
                {
                    ItemStack itemStack = itemControllers[j + indexOffset] as ItemStack;
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

                for (int j = 0; j < 20; j++)
                {
                    ItemStack itemStack = itemControllers[j + indexOffset] as ItemStack;
                    if (itemStack != null && (flag & (1 << j)) != 0)
                    {
                        itemStack.IsALockedSlot = true;
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

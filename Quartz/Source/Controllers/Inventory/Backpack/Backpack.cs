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

        protected BitArray lockedSlots;

		public BitArray LockedSlots
		{
			get { return lockedSlots; }
		}

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

			lockedSlots = new BitArray(itemControllers.Length);
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);

			if(player == null && XUi.IsGameRunning())
			{
				player = xui.playerUI.entityPlayer;

				LoadLockedSlots();
				SetLockedSlots();
				
				//if(comboBox != null)
				//{
				//	comboBox.Value = lockedSlots;
    //                OnLockedSlotsChange(this, 0, lockedSlots);
    //            }

				//if(standardControls != null)
				//{
				//	standardControls.ChangeLockedSlots(lockedSlots);
				//}
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
					itemStack.IsALockedSlot = i < newValue;
					lockedSlots.Set(i, i < newValue);
				}
			}

			SaveLockedSlots();
		}

		protected void SetLockedSlots()
		{
            for (int i = 0; i < itemControllers.Length; i++)
            {
                ItemStack itemStack = itemControllers[i] as ItemStack;
                if (itemStack != null)
                {
                    itemStack.IsALockedSlot = lockedSlots[i];
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

		protected virtual void SaveLockedSlots()
		{
            if (player == null)
            {
				return;
            }

            int saveArrayCount = lockedSlots.Count/20;

			if(lockedSlots.Count % 20 != 0)
			{
				saveArrayCount++;
			}

			for(int i = 0; i < saveArrayCount; i++)
			{
				int flag = 0;
				int indexOffset = i * 20;

				for(int j = 0; j < 20; j++)
				{
                    if (lockedSlots[j + indexOffset])
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

            int saveArrayCount = lockedSlots.Count / 20;

            if (lockedSlots.Count % 20 != 0)
            {
                saveArrayCount++;
            }

            for (int i = 0; i < saveArrayCount; i++)
            {
                int flag = (int)player.GetCVar(lockedSlotsCvarName + i);
                int indexOffset = i * 20;

                for (int j = 0; j < 20; j++)
                {
                    if ((flag & (1 << j)) != 0)
                    {
                        lockedSlots.Set(j + indexOffset, true);
                    }
                }
            }
        }
	}
}

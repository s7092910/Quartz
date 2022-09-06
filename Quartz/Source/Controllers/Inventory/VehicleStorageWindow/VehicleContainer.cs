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

using HarmonyLib;

namespace Quartz
{
	public class VehicleContainer : global::XUiC_VehicleContainer
	{
		private const string TAG = "VehicleContainer";

		private XUiC_ContainerStandardControls controls;
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
				ViewComponent.IsVisible = xui.vehicle.GetVehicle().HasStorage(); ;
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
				}
			}
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

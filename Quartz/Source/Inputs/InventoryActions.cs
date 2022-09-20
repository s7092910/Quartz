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


using InControl;
using Platform;

namespace Quartz.Inputs
{
    public class InventoryActions : PlayerActionsBase
    {
        public PlayerAction LockSlot;

        public InventoryActions()
        {
            Name = "inventoryActions";

            PlayerActionsLocal playerActionsLocal = PlatformManager.NativePlatform.Input.PrimaryPlayer;

            UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[]
            {
                playerActionsLocal.PermanentActions,
                playerActionsLocal.GUIActions
            });

            playerActionsLocal.PermanentActions.AddBindingConflictWithActionSet(this);
            playerActionsLocal.GUIActions.AddBindingConflictWithActionSet(this);
        }

        protected override void CreateActions()
        {
            LockSlot = CreatePlayerAction("Inventory Lock Slot");
            LockSlot.UserData = new PlayerActionData.ActionUserData("quartzSettingInputLockedSlots", "quartzSettingInputLockedSlotsTooltip", PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true);
        }

        protected override void CreateDefaultJoystickBindings()
        {
        }

        protected override void CreateDefaultKeyboardBindings()
        {
            LockSlot.AddDefaultBinding(new Key[]
            {
                Key.LeftAlt
            });
        }
    }
}

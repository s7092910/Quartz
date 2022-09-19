using InControl;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

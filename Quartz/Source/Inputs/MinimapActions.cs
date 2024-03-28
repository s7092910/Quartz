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
    public class MinimapActions : PlayerActionsBase
    {
        public PlayerAction MinimapZoomIn;
        public PlayerAction MinimapZoomOut;

        public PlayerAction MinimapToggle;

        public MinimapActions()
        {
            Name = "minimapActions";

            PlayerActionsLocal playerActionsLocal = PlatformManager.NativePlatform.Input.PrimaryPlayer;

            UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[]
            {
                playerActionsLocal,
                playerActionsLocal.PermanentActions
            });

            playerActionsLocal.AddBindingConflictWithActionSet(this);
            playerActionsLocal.PermanentActions.AddBindingConflictWithActionSet(this);
        }

        protected override void CreateActions()
        {
            MinimapZoomIn = CreatePlayerAction("Minimap Zoom In");
            MinimapZoomIn.UserData = new PlayerActionData.ActionUserData("quartzSettingInputMinimapZoomIn", "quartzSettingInputMinimapZoomInTooltip", PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true);

            MinimapZoomOut = CreatePlayerAction("Minimap Zoom Out");
            MinimapZoomOut.UserData = new PlayerActionData.ActionUserData("quartzSettingInputMinimapZoomOut", "quartzSettingInputMinimapZoomOutTooltip", PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true);

            MinimapToggle = CreatePlayerAction("Minimap Visibility Toggle");
            MinimapToggle.UserData = new PlayerActionData.ActionUserData("quartzSettingInputMinimapZoomOut", "quartzSettingInputMinimapZoomOutTooltip", PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true);
        }

        protected override void CreateDefaultJoystickBindings()
        {
        }

        protected override void CreateDefaultKeyboardBindings()
        {
            MinimapZoomIn.AddDefaultBinding(new Key[]
            {
                Key.PadPlus
            });

            MinimapZoomOut.AddDefaultBinding(new Key[]
            {
                Key.PadMinus
            });

            MinimapToggle.AddDefaultBinding(new Key[]
            {
                Key.PadEnter
            });
        }
    }
}

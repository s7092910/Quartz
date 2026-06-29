/*Copyright 2023 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using Quartz.Hud;

namespace Quartz
{
    public class XUiC_HUDStamina : XUiC_HUDStatbar
    {
        private bool isSprinting;

        [XuiXmlBinding("sprintactive")]
        public bool IsSprinting
        {
            get => isSprinting;
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (!XUi.IsGameRunning() || LocalPlayer == null)
            {
                return;
            }

            bool sprinting = localPlayer.MovementRunning || localPlayer.MoveController.RunToggleActive;
            if (isSprinting != sprinting)
            {
                isSprinting = sprinting;
                IsDirty = true;
            }

        }

        protected override float GetCurrentStat()
        {
            return XUiM_Player.GetStamina(LocalPlayer);
        }

        protected override float GetMaxStat()
        {
            return LocalPlayer.Stats.Stamina.Max;
        }

        protected override float GetModifiedMax()
        {
            return LocalPlayer.Stats.Stamina.ModifiedMax;
        }

        protected override float GetStatUIPercentage()
        {
            return LocalPlayer.Stats.Stamina.ValuePercentUI;
        }
    }
}

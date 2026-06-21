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
using UnityEngine;

namespace Quartz
{
    public class XUiC_HUDStealth : XUiC_HUDStatbar
    {
        private int buffOffset = 52;

        [XuiXmlAttribute("buff_offset")]
        public int BuffOffset 
        { 
            get => buffOffset; 
            set 
            {
                IsDirty |= buffOffset != value;
                buffOffset = value;
            }  
        }

        [XuiXmlBinding("stealthcolor")]
        public Color32 GetStealthColor()
        {
            return localPlayer ? localPlayer.Stealth.ValueColorUI : default;
        }

        protected override bool IsStatVisible()
        {
            if(base.IsStatVisible())
            {
                xui.BuffPopoutList.SetYOffset(LocalPlayer.Crouching ? BuffOffset : 0);
                return LocalPlayer.Crouching;
            }

            return false;
        }

        protected override float GetCurrentStat()
        {
            return LocalPlayer.Stealth.ValuePercentUI * 100f;
        }

        protected override float GetMaxStat()
        {
            return 100f;
        }

        protected override float GetModifiedMax()
        {
            return 100f;
        }

        protected override float GetStatUIPercentage()
        {
            return LocalPlayer.Stealth.ValuePercentUI;
        }
    }
}

﻿/*Copyright 2023 Christopher Beda

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
        public readonly CachedStringFormatterXuiRgbaColor stealthColorFormatter = new CachedStringFormatterXuiRgbaColor();

        private int buffOffset = 52;

        public override bool ParseAttribute(string attribute, string value, XUiController _parent)
        {
            if (attribute != null)
            {
                switch (attribute)
                {
                    case "buff_offset":
                        int temp = buffOffset;
                        int.TryParse(value, out buffOffset);
                        IsDirty |= temp != buffOffset;
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, _parent);
                }
            }

            return false;
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "stealthcolor":
                    value = stealthColorFormatter.Format(localPlayer ? localPlayer.Stealth.ValueColorUI : default);
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        protected override bool IsStatVisible()
        {
            if(base.IsStatVisible())
            {
                xui.BuffPopoutList.SetYOffset(LocalPlayer.Crouching ? buffOffset : 0);
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

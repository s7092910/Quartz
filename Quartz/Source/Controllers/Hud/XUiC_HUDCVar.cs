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
    public class XUiC_HUDCVar : XUiC_HUDStatbar
    {
        private string cvarName;
        private float maxValue;

        public override bool ParseAttribute(string attribute, string value, XUiController _parent)
        {
            if (attribute != null)
            {
                switch (attribute)
                {
                    case "cvar_name":
                        string tempString = cvarName;
                        cvarName = value;
                        IsDirty |= !tempString.EqualsCaseInsensitive(cvarName);
                        return true;
                    case "max_value":
                        float tempFloat = maxValue;
                        float.TryParse(value, out maxValue);
                        IsDirty |= tempFloat != maxValue;
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, _parent);
                }
            }

            return false;
        }

        protected override float GetCurrentStat()
        {
            return !string.IsNullOrEmpty(cvarName) ? LocalPlayer.GetCVar(cvarName) : 0;
        }

        protected override float GetMaxStat()
        {
            return maxValue;
        }

        protected override float GetModifiedMax()
        {
            return maxValue;
        }

        protected override float GetStatUIPercentage()
        {
            return !string.IsNullOrEmpty(cvarName) ? LocalPlayer.GetCVar(cvarName)/maxValue : 0;
        }
    }
}

/*Copyright 2026 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

namespace Quartz
{
    public class XUiC_PowerSourceStats : global::XUiC_PowerSourceStats
    {
        public readonly CachedStringFormatterInt maxBatteryCapacityFormatter = new CachedStringFormatterInt();
        public readonly CachedStringFormatterInt currentBatteryCapacityFormatter = new CachedStringFormatterInt();
        public readonly CachedStringFormatterFloat batteryCapacityFillFormatter = new CachedStringFormatterFloat();

        [XuiXmlBinding("showbattery")]
        public bool ShowBattery()
        {
            if (tileEntity == null)
            {
                return false;
            }

            return tileEntity.PowerItemType == PowerItem.PowerItemTypes.BatteryBank;
        }

        [XuiXmlBinding("maxbatterycapacity")]
        private int GetMaxBatteryCapacity()
        {
            if (tileEntity == null)
            {
                return 0;
            }

            int maxCapacity = 0;
            foreach(ItemStack itemStack in tileEntity.ItemSlots)
            {
                if (!itemStack.IsEmpty())
                {
                    maxCapacity += itemStack.itemValue.MaxUseTimes;
                }
            }

            return maxCapacity;
        }

        [XuiXmlBinding("batterycapacity")]
        private int GetCurrentBatteryCapacity()
        {
            if (tileEntity == null)
            {
                return 0;
            }

            int currentCapacity = 0;
            foreach (ItemStack itemStack in tileEntity.ItemSlots)
            {
                if (!itemStack.IsEmpty())
                {
                    currentCapacity += (int)(itemStack.itemValue.MaxUseTimes - itemStack.itemValue.UseTimes);
                }
            }

            return currentCapacity;
        }

        [XuiXmlBinding("batterycapacityfill")]
        private float GetBatteryCapacityFill()
        {
            if (tileEntity == null)
            {
                return 0f;
            }

            float maxCapacity = 0;
            float currentCapacity = 0;
            foreach (ItemStack itemStack in tileEntity.ItemSlots)
            {
                if (!itemStack.IsEmpty())
                {
                    maxCapacity += itemStack.itemValue.MaxUseTimes;
                    currentCapacity += itemStack.itemValue.MaxUseTimes - itemStack.itemValue.UseTimes;
                }
            }

            return currentCapacity/maxCapacity;
        }
    }
}

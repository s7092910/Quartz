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
        
        public override bool GetBindingValueInternal(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "showbattery":
                    value = tileEntity == null ? "false" : (tileEntity.PowerItemType == PowerItem.PowerItemTypes.BatteryBank).ToString();
                    return true;
                case "maxbatterycapacity":
                    value = GetMaxBatteryCapacity();
                        return true;
                case "batterycapacity":
                    value = GetCurrentBatteryCapacity();
                    return true;
                case "batterycapacityfill":
                    value = GetBatteryCapacityFill();
                    return true;
                default:
                    return base.GetBindingValueInternal(ref value, bindingName);
            }
        }

        private string GetMaxBatteryCapacity()
        {
            if (tileEntity == null)
            {
                return "0";
            }

            int maxCapacity = 0;
            foreach(ItemStack itemStack in tileEntity.ItemSlots)
            {
                if (!itemStack.IsEmpty())
                {
                    maxCapacity += itemStack.itemValue.MaxUseTimes;
                }
            }

            return maxBatteryCapacityFormatter.Format(maxCapacity);
        }

        private string GetCurrentBatteryCapacity()
        {
            if (tileEntity == null)
            {
                return "0";
            }

            int currentCapacity = 0;
            foreach (ItemStack itemStack in tileEntity.ItemSlots)
            {
                if (!itemStack.IsEmpty())
                {
                    currentCapacity += (int)(itemStack.itemValue.MaxUseTimes - itemStack.itemValue.UseTimes);
                }
            }

            return currentBatteryCapacityFormatter.Format(currentCapacity);
        }

        private string GetBatteryCapacityFill()
        {
            if (tileEntity == null)
            {
                return "0";
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

            return batteryCapacityFillFormatter.Format(currentCapacity/maxCapacity);
        }
    }
}

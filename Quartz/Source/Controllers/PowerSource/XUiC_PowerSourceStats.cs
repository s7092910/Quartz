using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

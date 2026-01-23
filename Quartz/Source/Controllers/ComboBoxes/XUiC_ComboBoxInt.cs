using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Generic.HashSetLong;

namespace Quartz
{
    public class XUiC_ComboBoxInt : global::XUiC_ComboBoxInt
    {
        public override void setRelativeValue(double _value)
        {
            long oldValue = Value;
            long value = (long)((double)(Max - Min) * _value) + Min;
            value = ((long)Math.Round(value / (double)IncrementSize)) * IncrementSize;
            Value = value;
            TriggerValueChangedEvent(oldValue);
        }

        [PublicizedFrom(EAccessModifier.Protected)]
        public override void incrementalChangeValue(double _value)
        {
            Logging.Inform("incremental change = " + _value);
            long oldValue = Value;
            long num = 0;
            if (_value > 0)
            {
                num = IncrementSize;
            }
            else if (_value < 0)
            {
                num = -IncrementSize;
            }

            long value = Value + num;
            if (Wrap)
            {
                if (value < Min)
                {
                    value = Max;
                }
                else if (value > Max)
                {
                    value = Min;
                }
            }
            Value = value;
            TriggerValueChangedEvent(oldValue);
        }
    }
}

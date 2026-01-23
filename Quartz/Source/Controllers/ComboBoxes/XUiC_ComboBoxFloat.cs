using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz
{
    public class XUiC_ComboBoxFloat : global::XUiC_ComboBoxFloat
    {
        public override void setRelativeValue(double _value)
        {
            double oldValue = Value;
            double value = (Max - Min) * _value + Min;
            value = Math.Round(value / IncrementSize) * IncrementSize;
            Value = value;
            TriggerValueChangedEvent(oldValue);
        }

        public override void incrementalChangeValue(double _value)
        {
            Logging.Inform("incremental change = " + _value);
            double oldValue = Value;
            double num = 0;
            if (_value > 0.0)
            {
                num = IncrementSize;
            }
            else if (_value < 0.0)
            {
                num = -IncrementSize;
            }

            double value = oldValue + num;
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

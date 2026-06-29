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

using System;

namespace Quartz
{
    public class XUiC_ComboBoxInt : global::XUiC_ComboBoxInt
    {
        public override double RelativeValue
        {
            set
            {
                long oldValue = Value;
                long newValue = (long)((double)(Max - Min) * value) + Min;
                newValue = ((long)Math.Round(newValue / (double)incrementSize)) * incrementSize;
                Value = newValue;
                TriggerValueChangedEvent(oldValue);
            }
        }

        [PublicizedFrom(EAccessModifier.Protected)]
        public override void incrementalChangeValue(double _value)
        {
            Logging.Inform("incremental change = " + _value);
            long oldValue = Value;
            long num = 0;
            if (_value > 0)
            {
                num = incrementSize;
            }
            else if (_value < 0)
            {
                num = -incrementSize;
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

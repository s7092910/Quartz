/*Copyright 2022 Christopher Beda

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
using UnityEngine;

namespace Quartz
{
    public class DialIndicator : XUiController
    {
        private const string TAG = "DialIndicator";

        private bool ClampValue = true;

        private float velocity = 0f;
        private float duration = 0.1f;

        private float indicatorValue;
        private float lastValue;
        private float rangeMax;
        private float rangeMin = 0f;
        private float valueRange;

        private float startAngle;
        private float endAngle;
        private float angleRange;

        private float indicatorAngle;


        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (ViewComponent.IsVisible && (IsDirty || lastValue != indicatorValue))
            {
                angleRange = calculateAngleRange(startAngle, endAngle);
                valueRange = rangeMax - rangeMin;

                lastValue = getLastValue(lastValue + 1, indicatorValue + 1) - 1;

                float iV = lastValue;
                if (ClampValue)
                {
                    iV = Mathf.Clamp(iV, rangeMin, rangeMax);
                }

                indicatorAngle = (iV * (angleRange / valueRange));
                indicatorAngle = startAngle - indicatorAngle;
                indicatorAngle %= 360;

                ViewComponent.UiTransform.localEulerAngles = new Vector3(0f, 0f, indicatorAngle);
                IsDirty = false;
            }

        }

        public override bool ParseAttribute(string attribute, string value, XUiController _parent)
        {
            float temp = 0;
            if (attribute != null)
            {
                switch (attribute)
                {
                    case "indicator_value":
                        temp = indicatorValue;
                        float.TryParse(value, out indicatorValue);
                        IsDirty |= temp != indicatorValue;
                        return true;
                    case "range_max":
                        temp = rangeMax;
                        float.TryParse(value, out rangeMax);
                        IsDirty |= temp != rangeMax;
                        return true;
                    case "range_min":
                        temp = rangeMin;
                        float.TryParse(value, out rangeMin);
                        IsDirty |= temp != rangeMin;
                        return true;
                    case "start_angle":
                        temp = startAngle;
                        float.TryParse(value, out startAngle);
                        IsDirty |= temp != startAngle;
                        return true;
                    case "end_angle":
                        temp = endAngle;
                        float.TryParse(value, out endAngle);
                        IsDirty |= temp != endAngle;
                        return true;
                    case "limit_indicator_to_range":
                        bool b = ClampValue;
                        ClampValue = StringParsers.ParseBool(value, 0, -1, true);
                        IsDirty |= b != ClampValue;
                        return true;
                    case "animation_duration":
                        float.TryParse(value, out duration);
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, _parent);
                }
            }

            return false;
        }

        public override void OnOpen()
        {
            base.OnOpen();
            IsDirty = true;
            lastValue = indicatorValue;
        }

        private float calculateAngleRange(float a, float b)
        {
            float angleRange = a - b;
            angleRange %= 360;

            if (angleRange < 0)
            {
                angleRange = Math.Abs(360 + angleRange);
            }

            return angleRange != 0 ? angleRange : 360f;
        }

        private float getLastValue(float current, float target)
        {
            float val = Mathf.SmoothDamp(current, target, ref velocity, duration);
            float diff = Math.Abs(target - val);
            if ((diff / target) < 0.005)
            {
                val = target;
                velocity = 0;
            }
            return val;
        }
    }
}
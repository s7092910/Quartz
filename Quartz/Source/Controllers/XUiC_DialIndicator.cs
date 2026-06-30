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
    public class XUiC_DialIndicator : XUiController
    {
        private const string TAG = "DialIndicator";

        private bool clampValue = true;

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


        [XuiXmlAttribute("animation_duration")]
        public float Duration 
        { 
            get => duration; 
            set => duration = value; 
        }

        [XuiXmlAttribute("indicator_value")]
        public float IndicatorValue 
        { 
            get => indicatorValue;
            set
            {
                IsDirty |= indicatorValue != value;
                indicatorValue = value;
            }
        }

        [XuiXmlAttribute("range_max")]
        public float RangeMax 
        { 
            get => rangeMax;
            set
            {
                IsDirty |= rangeMax != value;
                rangeMax = value;
            }
        }

        [XuiXmlAttribute("range_min")]
        public float RangeMin 
        { 
            get => rangeMin;
            set
            {
                IsDirty |= rangeMin != value;
                rangeMin = value;
            }
        }

        [XuiXmlAttribute("start_angle")]
        public float StartAngle 
        { 
            get => startAngle;
            set
            {
                IsDirty |= startAngle != value;
                startAngle = value;
            }
        }

        [XuiXmlAttribute("end_angle")]
        public float EndAngle 
        {
            get => endAngle;
            set
            {
                IsDirty |= endAngle != value;
                endAngle = value;
            }
        }

        [XuiXmlAttribute("limit_indicator_to_range")]
        public bool ClampValue 
        { 
            get => clampValue;
            set
            {
                IsDirty |= clampValue != value;
                clampValue = value;
            }
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (ViewComponent.IsVisible && (IsDirty || lastValue != indicatorValue))
            {
                angleRange = calculateAngleRange(startAngle, endAngle);
                valueRange = rangeMax - rangeMin;

                lastValue = getLastValue(lastValue + 1, indicatorValue + 1) - 1;

                float iV = lastValue;
                if (clampValue)
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
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

using UnityEngine;

namespace Quartz
{
    public class XUiC_Spinner : XUiController
    {
        private const string TAG = "Spinner";

        private bool isSpinning;
        private int spinSpeedAngle;
        private float rotation;

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (viewComponent.IsVisible && isSpinning)
            {
                float deltaAngle = _dt * spinSpeedAngle;
                rotation = (rotation + deltaAngle) % 360;
                if (rotation < 0)
                {
                    rotation += 360;
                }
                viewComponent.UiTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
            }

        }

        public override bool ParseAttribute(string attribute, string value, XUiController _parent)
        {
            if (attribute != null)
            {
                switch (attribute)
                {
                    case "spin":
                        isSpinning = StringParsers.ParseBool(value, 0, -1, true);
                        return true;
                    case "angle_per_second":
                        int.TryParse(value, out spinSpeedAngle);
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, _parent);
                }
            }
            return false;
        }
    }
}
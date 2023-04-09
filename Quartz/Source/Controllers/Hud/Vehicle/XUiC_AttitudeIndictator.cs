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

using Quartz.Utils;
using UnityEngine;

namespace Quartz
{
    public class XUiC_AttitudeIndicator : XUiController
    {
        private EntityPlayerLocal localPlayer;
        private EntityVehicle vehicle;
        private bool isInFlyingVehicle;

        private XUiController bankIndicator;
        private XUiController horizonIndicator;

        public EntityVehicle Vehicle 
        { 
            get => vehicle;
            internal set
            {
                if(vehicle != value)
                {
                    vehicle = value;
                    isInFlyingVehicle = vehicle.IsFlyingVehicle();
                }          
            }
        }

        public override void Init()
        {
            base.Init();
            bankIndicator = GetChildById("bankIndicator");
            horizonIndicator = GetChildById("horizon");
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (localPlayer == null)
            {
                localPlayer = xui.playerUI.entityPlayer;
            }

            if (XUi.IsGameRunning() && localPlayer != null && ViewComponent.IsVisible)
            {
                Vehicle = localPlayer.AttachedToEntity as EntityVehicle;
                if (isInFlyingVehicle)
                {
                    if(bankIndicator != null)
                    {
                        bankIndicator.ViewComponent.UiTransform.localEulerAngles = new Vector3(0f, 0f, 360 - vehicle.transform.eulerAngles.z);
                    }
                    if(horizonIndicator != null)
                    {
                        horizonIndicator.ViewComponent.UiTransform.localEulerAngles = new Vector3(0f, 0f, 360 - vehicle.transform.eulerAngles.z);
                    }
                    RefreshBindings(false);
                }
            }
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "angles":
                    value = "";
                    if(isInFlyingVehicle)
                    {
                        value = "x{" + vehicle.transform.eulerAngles.x + "}, y{" + vehicle.transform.eulerAngles.y + "}, z{" + (360 - vehicle.transform.eulerAngles.z) + "}";
                    }
                    return true;
                case "pitchangle":
                    value = "0";
                    if (isInFlyingVehicle)
                    {
                        int angle = (int)(vehicle.transform.eulerAngles.x);
                        //Changes the 0 - 360 range to - 180 to 180 and flips the angle. So 180 to 360 is 0 to 180 and 0 to 180 is 0 to -180 
                        //Postive is the nose pointing up
                        //Negative is the nose pointing down
                        angle += 180;
                        angle %= 360;
                        angle -= 180;
                        angle *= -1;
                        value = angle.ToString();
                    }
                    return true;
                case "rollangle":
                    value = "0";
                    if(isInFlyingVehicle)
                    {
                        int angle = (int)(vehicle.transform.eulerAngles.z);
                        //Changes the 0 - 360 range to - 180 to 180 and flips the angle. So 180 to 360 is 0 to 180 and 0 to 180 is 0 to -180 
                        //Postive the roll is the right
                        //Negative the roll is to the left
                        angle += 180;
                        angle %= 360;
                        angle -= 180;
                        angle *= -1;
                        value = angle.ToString();
                    }
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }
    }
}

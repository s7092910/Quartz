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

namespace Quartz
{
    public class XUiC_HUDVehicle : XUiController
    {
        private EntityPlayerLocal localPlayer;
        private EntityVehicle vehicle;
        private VPHeadlight headlight;
        private bool isInFlyingVehicle;
        private bool isDriving;
        private bool isHeadlightOn;

        public EntityVehicle Vehicle
        {
            get => vehicle;
            internal set
            {
                if (vehicle != value)
                {
                    vehicle = value;
                    isInFlyingVehicle = vehicle.IsFlyingVehicle();
                    headlight = vehicle.GetHeadlight();
                    IsDirty = true;
                }
            }
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (localPlayer == null)
            {
                localPlayer = xui.playerUI.entityPlayer;
            }

            if (XUi.IsGameRunning() && localPlayer != null)
            {
                Vehicle = localPlayer.AttachedToEntity as EntityVehicle;
                if(isDriving != IsDriver())
                {
                    IsDirty = true;
                    isDriving = IsDriver();
                }
            }

            if (IsDirty || isHeadlightOn != IsHeadlightOn())
            {
                isHeadlightOn = IsHeadlightOn();
                IsDirty = false;
                RefreshBindings();
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            IsDirty = true;
            RefreshBindings();
        }

        public override void OnClose()
        {
            base.OnClose();
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "invehicle":
                    value = localPlayer != null 
                        && !localPlayer.IsDead() 
                        && vehicle != null 
                        ? "true" : "false";
                    return true;
                case "isdriver":
                    value = IsDriver() ? "true" : "false";
                    return true;
                case "isaflyingvehicle":
                    value = isInFlyingVehicle ? "true" : "false";
                    return true;
                case "hasengine":
                case "hasfuel":
                    value = localPlayer != null
                        && !localPlayer.IsDead()
                        && vehicle != null
                        && vehicle.GetVehicle().HasEnginePart()
                        ? "true" : "false";
                    return true;
                case "hasheadlight":
                    value = localPlayer != null
                        && !localPlayer.IsDead()
                        && headlight != null
                        ? "true" : "false";
                    return true;
                case "isheadlighton":
                    value = localPlayer != null
                        && !localPlayer.IsDead()
                        && IsHeadlightOn()
                        ? "true" : "false";
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        private bool IsDriver()
        {
            return localPlayer != null 
                && !localPlayer.IsDead() 
                && vehicle != null 
                && vehicle.HasDriver 
                && vehicle.AttachedMainEntity == localPlayer;
        }

        private bool IsHeadlightOn()
        {
            return headlight != null && headlight.IsOn();
        }
    }
}

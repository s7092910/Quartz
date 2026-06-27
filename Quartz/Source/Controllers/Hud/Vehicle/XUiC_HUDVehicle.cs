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
        private bool isTurbo;

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

            if (IsDirty || isHeadlightOn != IsHeadlightOn() || isTurbo != IsTurboOn())
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

        [XuiXmlBinding("invehicle")]
        private bool IsInVehicle()
        {
            return localPlayer != null
                && !localPlayer.IsDead()
                && vehicle != null;
        }

        [XuiXmlBinding("isaflyingvehicle")]
        private bool IsFlyingVehicle()
        {
            return isInFlyingVehicle;
        }

        [XuiXmlBinding("hasengine")]
        private bool HasEngine()
        {
            return IsInVehicle()
                && vehicle.GetVehicle().HasEnginePart();
        }

        [XuiXmlBinding("hasfuel")]
        private bool HasFuel()
        {
            return HasEngine()
                && EntityVehicle.VehicleFuelUsageModifier > 0.0;
        }

        [XuiXmlBinding("isdriver")]
        private bool IsDriver()
        {
            return IsInVehicle()
                && vehicle.HasDriver 
                && vehicle.AttachedMainEntity == localPlayer;
        }

        [XuiXmlBinding("hasheadlight")]
        private bool HasHeadLights()
        {
            return IsInVehicle()
                   && headlight != null;
        }

        [XuiXmlBinding("isheadlighton")]
        private bool IsHeadlightOn()
        {
            return HasHeadLights()
                && headlight.IsOn();
        }

        [XuiXmlBinding("turboactive")]
        private bool IsTurboOn()
        {
            return IsInVehicle()
                && vehicle.vehicle.IsTurbo;
        }
    }
}

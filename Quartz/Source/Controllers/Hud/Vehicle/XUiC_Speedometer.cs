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

using System;

namespace Quartz
{
    public class XUiC_Speedometer : XUiController
    {
        private EntityPlayerLocal localPlayer;
        private EntityVehicle vehicle;

        private float cachedValue;

        public EntityVehicle Vehicle
        {
            get => vehicle;
            internal set
            {
                if (vehicle != value)
                {
                    vehicle = value;
                    IsDirty = true;
                }
            }
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
            }

            if(IsDirty || HasChanged())
            {
                RefreshBindings();
                IsDirty = false;
            }
        }

        protected bool HasChanged()
        {
            if (vehicle != null)
            {
                float value = vehicle.GetVelocityPerSecond().magnitude;

                bool result = cachedValue != value;
                cachedValue = value;

                return result;
            }

            return false;
        }

        [XuiXmlBinding("currentspeed")]
        private float GetCurrentSpeed()
        {
            float speed = 0f;
            if (localPlayer != null && vehicle != null)
            {
                speed = vehicle.GetVelocityPerSecond().magnitude * 3.6f;
            }

            return speed;
        }

        [XuiXmlBinding("currentforwardspeed")]
        private float GetCurrentForwardSpeed()
        {
            float speed = 0f;
            if (localPlayer != null && vehicle != null)
            {
                speed = Math.Abs(vehicle.GetVehicle().CurrentForwardVelocity * 3.6f);
            }

            return speed;
        }

        [XuiXmlBinding("maxspeed")]
        private float GetMaxSpeed()
        {
            float speed = 0f;
            if (localPlayer != null && vehicle != null)
            {
                speed = vehicle.GetVehicle().VelocityMaxForward * 3.6f;
            }

            return speed;
        }

        [XuiXmlBinding("maxspeedwithturbo")]
        private float GetMaxSpeedWithTurbo()
        {
            float speed = 0f;
            if (localPlayer != null && vehicle != null)
            {
                speed = vehicle.GetVehicle().MaxPossibleSpeed * 3.6f;
            }

            return speed;
        }

        [XuiXmlBinding("showspeed")]
        protected bool IsStatVisible()
        {
            return localPlayer != null && vehicle != null && !localPlayer.IsDead();

        }
    }
}

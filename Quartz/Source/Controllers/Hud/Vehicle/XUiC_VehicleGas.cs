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

using Quartz.Hud;

namespace Quartz
{
    public class XUiC_VehicleGas : XUiC_HUDStat
    {
        private EntityVehicle vehicle;
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
            if (XUi.IsGameRunning() && LocalPlayer != null)
            {
                Vehicle = LocalPlayer.AttachedToEntity as EntityVehicle;
            }
        }

        protected override bool HasChanged()
        {
            if (vehicle != null)
            {
                return base.HasChanged();
            }

            return false;
        }

        protected override bool IsStatVisible()
        {
            return base.IsStatVisible()
                && vehicle != null
                && vehicle.GetVehicle().HasEnginePart();
        }

        protected override string GetStatName()
        {
            return "gas";
        }

        protected override float GetStatUIPercentage()
        {
            return vehicle != null ? vehicle.GetVehicle().GetFuelPercent() : 0;
        }

        protected override float GetCurrentStat()
        {
            return vehicle != null ? vehicle.GetVehicle().GetFuelLevel() : 0;
        }

        protected override float GetMaxStat()
        {
            return vehicle != null ? vehicle.GetVehicle().GetMaxFuelLevel() : 0;
        }

        protected override float GetModifiedMax()
        {
            return vehicle != null ? vehicle.GetVehicle().GetMaxFuelLevel() : 0;
        }
    }
}

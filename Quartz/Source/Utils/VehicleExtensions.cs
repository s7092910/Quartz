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

using System.Linq;
using UnityEngine;

namespace Quartz.Utils
{
    public static class VehicleExtensions
    {
        public static bool IsFlyingVehicle(this EntityVehicle vehicle)
        {
            if(vehicle == null)
            {
                return false;
            }

            if (vehicle is EntityVGyroCopter || vehicle is EntityVHelicopter || vehicle is EntityVBlimp)
            {
                return true;
            }

            var properties = vehicle.GetVehicle().Properties.Classes.Dict
                .Where(entry => entry.Key.Contains("force"))
                .Select(item => item.Value);
            
            foreach(DynamicProperties property in properties)
            {
                string trigger = property.GetString("trigger");
                if (trigger != null && (trigger.Contains("motor") || trigger.Contains("inputForward")))
                {
                    Vector3 force = Vector3.zero;
                    property.ParseVec("force", ref force);
                    if(force.y > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

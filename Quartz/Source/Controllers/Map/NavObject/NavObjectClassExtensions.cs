﻿/*Copyright 2024 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using Quartz.Settings;

namespace Quartz.Map
{
    public static class NavObjectClassExtensions
    {
        public static bool IsOnMiniMap(this NavObjectClass instance, bool isActive)
        {
            NavObjectMapSettings mapSettings = instance.GetMapSettings(isActive);

            if(mapSettings == null)
            {
                return false;
            }

            if(!mapSettings.Properties.Contains("minimap_only"))
            {
                return true;
            }

            return MinimapSettings.ShowMinimapOnlyIcons || !mapSettings.Properties.GetBool("minimap_only");
        }
    }
}

using GearsAPI.Settings.Global;
/*Copyright 2024 Christopher Beda

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Settings
{
    public class MinimapSettings
    {
        public static bool Enabled { get; set; } = true;

        public static bool FollowPlayerView { get; set; } = false;

        public static void SetMinimapEnabled(IGlobalModSetting setting, string newValue)
        {
            Enabled = newValue == "Enabled";
        }

        public static void SetMinimapFollowsPlayerView(IGlobalModSetting setting, string newValue)
        {
            FollowPlayerView = newValue == "Enabled";
        }
    }
}

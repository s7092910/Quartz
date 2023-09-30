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

using Gears.SettingsManager.Settings;

namespace Quartz.Settings
{
    public class GlobalSettings
    {
        internal static UILabel.Crispness textCrispness = UILabel.Crispness.OnDesktop;

        public static void SetTextResolution(IModSetting setting, string newValue)
        {
            if(newValue == "High")
            {
                textCrispness = UILabel.Crispness.OnDesktop;
            } 
            else
            {
                textCrispness = UILabel.Crispness.Never;
            }
        }

        public static void SetDebugMode(IModSetting setting, string newValue)
        {
            if(newValue == "Enabled")
            {
                Debugging.EnableDebugging();
            }
            else
            {
                Debugging.DisableDebugging();
            }
        }
    }
}

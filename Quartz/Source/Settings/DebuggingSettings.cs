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

using GearsAPI.Settings.Global;
using Quartz.Debugging;
using UnityEngine;

namespace Quartz.Settings
{
    public static class DebuggingSettings
    {
        public static bool IsDebugEnabled { get; private set; } = false;

        public static void SetDebugMode(IGlobalModSetting setting, string newValue)
        {
            if (newValue == "Enabled")
            {
                EnableDebugging();
            }
            else
            {
                DisableDebugging();
            }
        }

        public static bool ToggleDebugging()
        {
            if(!IsDebugEnabled)
            {
                EnableDebugging();
            } 
            else
            {
                DisableDebugging();
            }

            return IsDebugEnabled;
        }

        public static void EnableDebugging()
        {
            if(IsDebugEnabled)
            {
                return;
            }

            IsDebugEnabled = true;

            QuartzDebug.DebugRaycast = true;
            Logging.enabled = true;
        }

        public static void DisableDebugging()
        {
            if(!IsDebugEnabled)
            {
                return;
            }

            IsDebugEnabled = false;

            QuartzDebug.DebugRaycast = true;
            Logging.enabled = false;
        }

        public static void SetFontSize(int fontSize)
        {
            QuartzDebug.FontSize = fontSize;
        }

        public static void SetFontColor(string fontColor)
        {
            Color color = Color.white;
            switch (fontColor.ToLower()) 
            {
                case "red":
                    color = Color.red;
                    break;
                case "green":
                    color = Color.green;
                    break;
                case "blue":
                    color = Color.blue;
                    break;
                case "white":
                    color = Color.white;
                    break;
                case "black":
                    color = Color.black;
                    break;
                case "yellow":
                    color = Color.yellow;
                    break;
                case "cyan":
                    color = Color.cyan;
                    break;
                case "magenta":
                    color = Color.blue;
                    break;
                case "gray":
                case "grey":
                    color = Color.grey;
                    break;
            }
            QuartzDebug.FontColor = color;
        }
    }
}

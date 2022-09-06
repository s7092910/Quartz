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

using System;

namespace Quartz
{
    public static class Logging
    {
        public static bool enabled = false;
        public static string logTag = "Quartz";

        public static void Out(string message)
        {
            if (enabled)
            {
                string mess = string.Format("[{0}] {1}", logTag, message);
                Log.Out(mess);
            }
        }

        public static void Out(string TAG, string message)
        {
            if (enabled)
            {
                string mess = string.Format("[{0}] [{1}] {2}", logTag, TAG, message);
                Log.Out(mess);
            }
        }

        public static void Inform(string message)
        {
            string mess = string.Format("[{0}] {1}", logTag, message);
            Log.Out(mess);
        }

        public static void Inform(string TAG, string message)
        {
            string mess = string.Format("[{0}] [{1}] {2}", logTag, TAG, message);
            Log.Out(mess);
        }

        public static void Warning(string message)
        {
            string mess = string.Format("[{0}] {1}", logTag, message);
            Log.Warning(mess);
        }

        public static void Warning(string TAG, string message)
        {
            string mess = string.Format("[{0}] [{1}] {2}", logTag, TAG, message);
            Log.Warning(mess);
        }


        public static void Error(string message)
        {
            string mess = string.Format("[{0}] {1}", logTag, message);
            Log.Error(mess);
        }

        public static void Error(string TAG, string message)
        {
            string mess = string.Format("[{0}] [{1}] {2}", logTag, TAG, message);
            Log.Error(mess);
        }
    }
}
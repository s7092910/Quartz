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

using System.IO;

namespace Quartz.Inputs
{
    public static class QuartzInputManager
    {
        private const string TAG = "QuartzInputManager";
        private const string saveName = "/ActionSetSaves.pref";
        private const string endChar = "-";

        //Increment currentVersion if more action sets have been added
        private const string currentVersion = "2";

        private static string saveFile;
        private static bool initCalled;

        public static InventoryActions inventoryActions;
        public static MinimapActions minimapActions;

        public static void InitControls(string ModPath)
        {
            if (GameManager.IsDedicatedServer && initCalled)
            {
                return;
            }

            saveFile = ModPath + saveName;

            initCalled = true;

            LoadActionSets();
            LoadControlSaves();
            SaveControls();
        }

        private static void LoadActionSets()
        {
            inventoryActions = new InventoryActions();
            minimapActions = new MinimapActions();
        }

        private static void LoadControlSaves()
        {
            if (!File.Exists(saveFile))
            {
                return;
            }
            string content = File.ReadAllText(saveFile);
            string[] ActionSetData = content.Split(';');
            
            if(ActionSetData.Length <= 0)
            {
                return;
            }

            // Legacy Version 1
            if(ActionSetData.Length == 2 ) 
            {
                inventoryActions.Load(ActionSetData[0]);
                return;
            }

            int version;
            if(!int.TryParse(ActionSetData[0], out version))
            {
                Logging.Inform("ActionSets file version not found");
                return;
            }

            Logging.Inform("ActionSets file version: " + version);

            if (version >= 2)
            {
                inventoryActions.Load(ActionSetData[2]);
                minimapActions.Load(ActionSetData[3]);
            }

            //Increment currentVersion if more action sets have been added
        }

        public static void SaveControls()
        {
            if(!initCalled)
            {
                return;
            }

            string[] actionSetSaveData = new string[5];
            actionSetSaveData[0] = currentVersion;
            actionSetSaveData[1] = endChar;
            actionSetSaveData[2] = inventoryActions.Save();
            actionSetSaveData[3] = minimapActions.Save();
            actionSetSaveData[4] = endChar;

            string saveData = string.Join(";", actionSetSaveData);
            File.WriteAllText(saveFile, saveData);
        }
    }
}

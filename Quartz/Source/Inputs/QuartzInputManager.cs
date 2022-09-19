using Quartz.Inputs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Source.Inputs
{
    public static class QuartzInputManager
    {
        private const string TAG = "QuartzInputManager";
        private const string saveName = "/ActionSetSaves.pref";

        private static string saveFile;
        private static bool initCalled;

        public static InventoryActions inventoryActions;

        public static void InitCustomControls(string ModPath)
        {
            if (GameManager.IsDedicatedServer && initCalled)
            {
                return;
            }

            saveFile = ModPath + saveName;
            
            LoadActionSets();
            LoadControlSaves();
            SaveControls();

            initCalled = true;
        }

        private static void LoadActionSets()
        {
            inventoryActions = new InventoryActions();
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

            inventoryActions.Load(ActionSetData[0]);
        }

        public static void SaveControls()
        {
            if(!initCalled)
            {
                return;
            }

            string[] actionSetSaveData = new string[1];
            actionSetSaveData[0] = inventoryActions.Save();

            string saveData = string.Join(";", actionSetSaveData);
            File.WriteAllText(saveFile, saveData);
        }
    }
}

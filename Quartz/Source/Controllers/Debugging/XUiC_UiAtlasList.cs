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

using Quartz.Utils;
using Quartz.Views;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quartz.Debugging
{
    public class XUiC_UiAtlasList : XUiC_List<XUiC_UiAtlasList.ListEntry>
    {
        public const string TAG = "UIAtlastList";

        public static string ID = "";

        private UIAtlas uiAtlas;
        private List<UISpriteData> spriteList;

        private XUiC_SimpleButton btnBack;
        private XUiC_SimpleButton btnExport;
        private XUiC_SimpleButton btnExplorer;
        private XUiController btnShowModSprites;

        private XUiController[] spriteControllers;

        private bool showModSprites = false;

        public override void Init()
        {
            base.Init();
            XUiC_UiAtlasList.ID = WindowGroup.ID;

            btnBack = GetChildById("btnBack") as XUiC_SimpleButton;
            if (btnBack != null )
            {
                btnBack.OnPressed += BackOnPress;
            }

            btnExport = GetChildById("btnExport") as XUiC_SimpleButton;
            if (btnExport != null)
            {
                btnExport.OnPressed += ExportOnPress;
            }

            btnExplorer = GetChildById("btnExplorer") as XUiC_SimpleButton;
            if(btnExplorer != null)
            {
                btnExplorer.OnPressed += ExplorerOnPress;
            }

            btnShowModSprites= GetChildById("btnShowModSprites");
            if(btnShowModSprites != null)
            {
                btnShowModSprites.OnPress += ShowModSpritesOnPress;
            }

            XUiController list = GetChildById("list");
            if (list != null)
            {
                spriteControllers = list.GetChildrenById("sprite");
            }
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);

            if(!showModSprites)
            {
                ShowVanillaSprites();
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            IsDirty = true;
            windowGroup.openWindowOnEsc = (GameStats.GetInt(EnumGameStats.GameState) == 0) ? XUiC_MainMenu.ID : null;
            XUi.InGameMenuOpen = true;

            RebuildList(false);
        }

        public override void OnClose()
        {
            base.OnClose();
            XUi.InGameMenuOpen = false;
        }

        public override void RebuildList(bool _resetFilter = false)
        {
            allEntries.Clear();
            RebuildListInternal();
            base.RebuildList(_resetFilter);
            XUiV_ScrollView scrollView = GetChildById("scrollview")?.ViewComponent as XUiV_ScrollView;

            if (scrollView != null)
            {
                scrollView.ForceResetPosition();
            }
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "spriteCount":
                    if (spriteList == null)
                    {
                        LoadSpriteData();
                    }
                    value = spriteList.Count.ToString();
                    Logging.Out(TAG, "Sprite Count = " + value);
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        private void RebuildListInternal()
        {
            if(spriteList == null)
            {
                LoadSpriteData();
            }

            foreach (UISpriteData spriteData in spriteList)
            {
                allEntries.Add(new ListEntry(spriteData));
            }
        }

        private void LoadSpriteData()
        {
            uiAtlas = xui.GetAtlasByName("UIAtlas", "menu_empty");
            if (uiAtlas != null)
            {
                Logging.Inform(TAG, "UIAtlas found");
                spriteList = uiAtlas.spriteList;
            }
            else
            {
                spriteList = new List<UISpriteData>();
            }
        }

        private void BackOnPress(XUiController _sender, int _mouseButton)
        {
            xui.playerUI.windowManager.Close(ID);
            if (GameStats.GetInt(EnumGameStats.GameState) == 0)
            {
                xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
            }
            XUi.InGameMenuOpen = false;
        }

        private void ExportOnPress(XUiController _sender, int _mouseButton)
        {
            if (uiAtlas == null)
            {
                Logging.Warning(TAG, "Unable to Export UIAtlas as UIAtlas is null");
                return;
            }

            string exportPath = GameIO.GetUserGameDataDir() + "/Quartz";
            if (!SdDirectory.Exists(exportPath))
            {
                SdDirectory.CreateDirectory(exportPath);
            }

            exportPath = exportPath + "/UIAtlasExport__" + DateTime.Now.ToString("yyyy'-'MM'-'dd'__'HH'-'mm'-'ss");
            if (!SdDirectory.Exists(exportPath))
            {
                SdDirectory.CreateDirectory(exportPath);
            }

            Logging.Inform(TAG, "Exporting UIAtlas");

            string currentPath = exportPath + "/UIAtlas.png";

            Texture2D texture = uiAtlas.texture.TextureFromGPU();
            Color32[] textPixels = texture.GetPixels32();

            byte[] bytes = ImageConversion.EncodeToPNG(texture);
            SdFile.WriteAllBytes(currentPath, bytes);

            foreach(UISpriteData sprite in spriteList)
            {
                int xmin = Mathf.Clamp(sprite.x, 0, texture.width);
                int ymin = Mathf.Clamp(sprite.y, 0, texture.height);
                int newWidth = Mathf.Clamp(sprite.width, 0, texture.width);
                int newHeight = Mathf.Clamp(sprite.height, 0, texture.height);
                if (newWidth == 0 || newHeight == 0)
                {
                    continue;
                }

                Color32[] newPixels = new Color32[newWidth * newHeight];

                for (int y = 0; y < newHeight; ++y)
                {
                    for (int x = 0; x < newWidth; ++x)
                    {
                        int newIndex = (newHeight - 1 - y) * newWidth + x;
                        int pixelIndex = (texture.height - 1 - (ymin + y)) * texture.width + (xmin + x);
                        newPixels[newIndex] = textPixels[pixelIndex];
                    }
                }

                Texture2D tex = new Texture2D(newWidth, newHeight);
                tex.SetPixels32(newPixels);
                tex.Apply(false, false);
                bytes = ImageConversion.EncodeToPNG(tex);

                currentPath = exportPath + "/" + sprite.name + ".png";
                SdFile.WriteAllBytes(currentPath, bytes);
            }

            Logging.Inform(TAG, "Completed Exporting UIAtlas");
        }

        private void ExplorerOnPress(XUiController _sender, int _mouseButton)
        {
            string exportPath = GameIO.GetUserGameDataDir() + "/Quartz";
            if(!SdDirectory.Exists(exportPath))
            {
                SdDirectory.CreateDirectory(exportPath);
            }
            GameIO.OpenExplorer(exportPath);
        }

        private void ShowModSpritesOnPress(XUiController _sender, int _)
        {
            showModSprites = !showModSprites;

            if(showModSprites)
            {
                ShowModSprites();
            } 
            else
            {
                ShowVanillaSprites();
            }
        }

        private void ShowModSprites()
        {
            foreach (XUiController controller in spriteControllers)
            {
                if (controller.ViewComponent is XUiV_Sprite sprite)
                {
                    sprite.SetSpriteImmediately(sprite.SpriteName);
                }
            }
        }

        private void ShowVanillaSprites()
        {
            foreach (XUiController controller in spriteControllers)
            {
                if (controller.ViewComponent is XUiV_Sprite sprite)
                {
                    sprite.Sprite.atlas = uiAtlas;
                }
            }
        }


        public class ListEntry : XUiListEntry<ListEntry>
        {
            private UISpriteData spriteData;

            public ListEntry(UISpriteData spriteData)
            {
                this.spriteData = spriteData;
            }

            public override int CompareTo(ListEntry otherEntry)
            {
                if (otherEntry is ListEntry entry)
                {
                    return string.Compare(spriteData.name, entry.spriteData.name);
                }

                return 1;
            }
            public override bool GetBindingValue(ref string value, string bindingName)
            {
                switch (bindingName)
                {
                    case "sprite":
                        value = spriteData.name;
                        return true;
                    case "spriteName":
                        value = spriteData.name;
                        return true;
                    case "hasentry":
                        value = "True";
                        return true;
                    default:
                        return false;
                }
            }

            public static bool GetNullBindingValues(ref string value, string bindingName)
            {
                switch (bindingName)
                {
                    case "sprite":
                    case "spriteName":
                        value = "";
                        return true;
                    case "hasentry":
                        value = "False";
                        return true;
                    default:
                        return false;
                }
            }

            public override bool MatchesSearch(string searchString)
            {
                return spriteData.name.Contains(searchString);
            }
        }
    }
}

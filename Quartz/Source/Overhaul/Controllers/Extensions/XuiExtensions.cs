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

using Quartz;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace QuartzOverhaul.Extensions
{
    public static class XuiExtensions
    {

        private static Dictionary<string, NGUIFont> fonts = new Dictionary<string, NGUIFont>();

        public static T GetChildByInterface<T>(this XUi xui) where T : class
        {
            for (int i = 0; i < xui.WindowGroups.Count; i++)
            {
                if(xui.WindowGroups[i].Controller is XUiBaseController child)
                {
                    T childByType = child.GetChildByInterface<T>();
                    if (childByType != null)
                    {
                        return childByType;
                    }
                }
            }

            return null;
        }

        public static NGUIFont GetNGUIFontByName(this XUi xui, string name)
        {

            if (fonts.Count == 0)
            {
                foreach (NGUIFont nguiFont in xui.NGUIFonts)
                {
                    fonts.Add(nguiFont.name, nguiFont);

                    if (nguiFont.name != nguiFont.spriteName)
                    {
                        fonts.Add(nguiFont.spriteName, nguiFont);
                    }
                }
            }

            NGUIFont font;

            if (fonts.TryGetValue(name, out font))
            {
                return font;
            }

            if (name.Contains("@modfolder("))
            {
                Font loadedFont = DataLoader.LoadAsset<Font>(name);

                if(loadedFont != null)
                {
                    font = ScriptableObject.CreateInstance<NGUIFont>();
                    font.name = name;
                    font.dynamicFont = loadedFont;

                    fonts.Add(name, font);

                    return font;
                } 
                //else 
                //{
                //    string path = ModManager.PatchModPathString(name);
                //    if(path != null && File.Exists(path))
                //    {
                //        FontEngine.LoadFontFace(path);
                //        loadedFont = new Font("file://" + name);

                //        foreach (string fontname in loadedFont.fontNames)
                //        {
                //            Logging.Inform("Font Name = " + fontname);
                //        }

                //        foreach (CharacterInfo characterInfo in loadedFont.characterInfo)
                //        {
                //            Logging.Inform("Character Info = " + characterInfo.ToString());
                //        }

                //        if (loadedFont != null)
                //        {
                //            font = ScriptableObject.CreateInstance<NGUIFont>();
                //            font.name = name;
                //            font.dynamicFont = loadedFont;

                //            fonts.Add(name, font);

                //            return font;
                //        }
                //    }
                //}
            }
            else
            {
                return TryLoadOSInstalledFont(name);
            }

            return null;
        }

        public static NGUIFont TryLoadOSInstalledFont(string fontName)
        {
            NGUIFont font = null;
            string[] osFonts = Font.GetOSInstalledFontNames();
            Font loadedFont = null;

            foreach (string osFont in osFonts)
            { 
                if (osFont == fontName)
                {
                    loadedFont = Font.CreateDynamicFontFromOSFont(osFont, 30);
                }
            }

            if (loadedFont != null)
            {
                font = ScriptableObject.CreateInstance<NGUIFont>();
                font.name = fontName;
                font.dynamicFont = loadedFont;

                fonts.Add(fontName, font);

                return font;
            }

            return null;

        }
    }
}

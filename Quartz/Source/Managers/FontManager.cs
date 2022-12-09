using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace Quartz.Managers
{
    public static class FontManager
    {
        private static Dictionary<string, NGUIFont> fonts = new Dictionary<string, NGUIFont>();

        private static NGUIFont referenceFont;

        private static bool loaded;

        private const string TAG = "FontManager";

        public const string styleKeyNGUIFonts = "Fonts.NGUIFonts";
        public const string styleKeyUnityFonts = "Fonts.UnityFonts";
        public const string styleKeyOSFonts = "Fonts.OSFonts";

        public static NGUIFont GetVanillaFont()
        {
            return referenceFont;
        }

        public static IEnumerator LoadFonts(XUi xui, Dictionary<string, XUiFromXml.StyleData> styles)
        {
            yield return null;
            while (!XUiFromXml.HasData())
            {
                yield return null;
            }

            yield return null;

            Logging.Inform("Loading Fonts");
            bool loadedXUIFonts = FontManager.LoadXUiFonts(xui);
            if (!loadedXUIFonts)
            {
                Logging.Warning("Unable to load XUi Fonts");
            }

            XUiFromXml.StyleData fontData;
            if (styles.TryGetValue(FontManager.styleKeyNGUIFonts, out fontData))
            {
                foreach (XUiFromXml.StyleEntryData fontEntry in fontData.StyleEntries.Values)
                {
                    bool sucess = FontManager.LoadNGUIFont(fontEntry.Name, fontEntry.Value);

                    if (!sucess)
                    {
                        Logging.Warning("Unable to load Font: " + fontEntry.Name);
                    }
                }
            }

            if (styles.TryGetValue(FontManager.styleKeyUnityFonts, out fontData))
            {
                foreach (XUiFromXml.StyleEntryData fontEntry in fontData.StyleEntries.Values)
                {
                    bool sucess = FontManager.LoadUnityFont(fontEntry.Name, fontEntry.Value);

                    if (!sucess)
                    {
                        Logging.Warning("Unable to load Font: " + fontEntry.Name);
                    }
                }
            }

            if (styles.TryGetValue(FontManager.styleKeyOSFonts, out fontData))
            {
                foreach (XUiFromXml.StyleEntryData fontEntry in fontData.StyleEntries.Values)
                {
                    bool sucess = FontManager.LoadOSInstalledFont(fontEntry.Value);

                    if (!sucess)
                    {
                        Logging.Warning("Unable to load Font: " + fontEntry.Name);
                    }
                }
            }

            Logging.Inform("Loaded Fonts");

            yield break;
        }

        public static bool LoadXUiFonts(XUi xui)
        {
            if (!loaded)
            {
                foreach (NGUIFont nguiFont in xui.NGUIFonts)
                {
                    fonts.Add(nguiFont.name, nguiFont);

                    if (nguiFont.name != nguiFont.spriteName)
                    {
                        fonts.Add(nguiFont.spriteName, nguiFont);
                    }

                    if (nguiFont.name == "ReferenceFont")
                    {
                        referenceFont = nguiFont;
                    }
                }

                loaded = true;
            }

            return loaded;
        }

        public static NGUIFont GetNGUIFontByName(string name)
        {
            NGUIFont font = null;
            fonts.TryGetValue(name, out font);

            return font;
        }

        public static bool LoadUnityFont(string fontName, string path)
        {
            if (fonts.ContainsKey(fontName))
            {
                return true;
            }

            NGUIFont font = null;

            if (path.Contains("@modfolder("))
            {
                Font loadedFont = DataLoader.LoadAsset<Font>(path);

                if (loadedFont != null)
                {
                    font = ScriptableObject.CreateInstance<NGUIFont>();
                    font.name = fontName;
                    font.dynamicFont = loadedFont;

                    fonts.Add(fontName, font);
                    Logging.Inform(TAG, "Font: " + fontName + " loaded");
                }
            }

            return font != null;
        }

        public static bool LoadNGUIFont(string fontName, string path)
        {
            if (fonts.ContainsKey(fontName))
            {
                return true;
            }

            NGUIFont font = null;

            if (path.Contains("@modfolder("))
            {
                font = DataLoader.LoadAsset<NGUIFont>(path);

                if (font != null)
                {
                    fonts.Add(fontName, font);
                    Logging.Inform(TAG, "Font: " + fontName + " loaded");
                }
            }

            return font != null;
        }

        public static bool LoadOSInstalledFont(string fontName)
        {
            if (fonts.ContainsKey(fontName))
            {
                return true;
            }

            string[] osFonts = Font.GetOSInstalledFontNames();
            Font loadedFont = null;
            NGUIFont font = null;

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
                Logging.Inform(TAG, "Font: " + fontName + " loaded");
            }

            return font != null;

        }
    }
}

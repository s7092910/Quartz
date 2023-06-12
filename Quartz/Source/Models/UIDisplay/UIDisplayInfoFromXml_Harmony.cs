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

using HarmonyLib;
using System;
using System.Xml.Linq;

[HarmonyPatch(typeof(UIDisplayInfoFromXml))]
public class UIDisplayInfoFromXmlPatch
{
    private const string TAG = "UIDisplayInfoFromXmlPatch";

    [HarmonyPrefix]
    [HarmonyPatch("ParseDisplayInfoEntry")]
    public static bool ParseDisplayInfoEntry(ref DisplayInfoEntry __result, XElement node)
    {
        Quartz.Models.DisplayInfoEntry displayInfoEntry = new Quartz.Models.DisplayInfoEntry();
        if (node.HasAttribute("name"))
        {
            string attribute = node.GetAttribute("name");
            try
            {
                displayInfoEntry.StatType = EnumUtils.Parse<PassiveEffects>(attribute, _ignoreCase: true);
            }
            catch
            {
                displayInfoEntry.CustomName = attribute;
            }
        }

        if (node.HasAttribute("display_type"))
        {
            displayInfoEntry.DisplayType = EnumUtils.Parse<DisplayInfoEntry.DisplayTypes>(node.GetAttribute("display_type"), _ignoreCase: true);
        }

        if (node.HasAttribute("show_inverted"))
        {
            displayInfoEntry.ShowInverted = Convert.ToBoolean(node.GetAttribute("show_inverted"));
        }

        if (node.HasAttribute("title_key"))
        {
            displayInfoEntry.TitleOverride = Localization.Get(node.GetAttribute("title_key"));
        }

        if (node.HasAttribute("negative_preferred"))
        {
            displayInfoEntry.NegativePreferred = Convert.ToBoolean(node.GetAttribute("negative_preferred"));
        }

        if (node.HasAttribute("display_leading_plus"))
        {
            displayInfoEntry.DisplayLeadingPlus = Convert.ToBoolean(node.GetAttribute("display_leading_plus"));
        }

        if (node.HasAttribute("tags"))
        {
            displayInfoEntry.Tags = FastTags.Parse(node.GetAttribute("tags"));
        }

        if (node.HasAttribute("icon"))
        {
            displayInfoEntry.icon = node.GetAttribute("icon");
        }

        __result = displayInfoEntry;

        return false;
    }
}

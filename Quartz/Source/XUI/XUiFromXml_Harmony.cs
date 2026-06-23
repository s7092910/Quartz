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

using HarmonyLib;
using Quartz;
using Quartz.Views;
using System.Collections.Generic;
using System.Xml.Linq;

[HarmonyPatch(typeof(XUiFromXml))]
public class XUiFromXmlPatch
{
    private const string TAG = "XUiFromXmlPatch";

    [HarmonyPrefix]
    [HarmonyPatch(nameof(XUiFromXml.createView))]
    public static bool createView(XUi _xui, string _name, string _type, XElement _node, XUiController _parent, 
        XUiWindowGroup _windowGroup, Dictionary<string, object> _templateParams, 
        ref bool _parseChildren, ref bool _parseControllerAndAttributes, ref bool _replacedByTemplate,
        ref XUiView __result)
    {
        XUiView view = null;

        switch(_type)
        {
            case "curvedlabel":
                view = new XUiV_CurvedLabel(_xui, _name);
                break;
            case "maskedtexture":
                view = new XUiV_MaskedTexture(_xui, _name);
                break;
            case "maskedpanel":
                view = new XUiV_MaskedPanel(_xui, _name);
                break;
            case "animatedsprite":
                view = new XUiV_AnimatedSprite(_xui, _name);
                break;
        }

        if(view != null)
        {
            __result = view;
            return false;
        }

        return true;
    }
}

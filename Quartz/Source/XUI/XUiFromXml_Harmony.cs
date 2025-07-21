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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

[HarmonyPatch(typeof(XUiFromXml))]
public class XUiFromXmlPatch
{
    private const string TAG = "XUiFromXmlPatch";

    [HarmonyPrefix]
    [HarmonyPatch(nameof(XUiFromXml.parseViewComponents))]
    public static bool parseByElementName(ref XUiView __result,
        XElement _node, XUiController _parent, XUiWindowGroup _windowGroup,
        string nodeNameOverride = "", Dictionary<string, object> _controlParams = null)
    {
        string localName = _node.Name.LocalName;
        string id = localName;

        if (nodeNameOverride == "" && _node.HasAttribute("name"))
        {
            id = _node.GetAttribute("name");
        }
        else if (nodeNameOverride != "")
        {
            id = nodeNameOverride;
        }

        if (_controlParams != null)
        {
            XUiFromXmlReversePatch.parseControlParams(_node, _controlParams);
        }

        XUiView view = null;

        switch(localName)
        {
            case "curvedlabel":
                view = new XUiV_CurvedLabel(id);
                break;
            case "videoplayer":
                view = new XUiV_VideoPlayer(id);
                break;
            case "maskedtexture":
                view = new XUiV_MaskedTexture(id);
                break;
            case "maskedpanel":
                view = new XUiV_MaskedPanel(id);
                break;
            case "animatedsprite":
                view = new XUiV_AnimatedSprite(id);
                break;
            case "scrollview":
                view = new XUiV_ScrollViewContainer(id);
                break;
            case "scrollbar":
                view = new XUiV_ScrollBar(id);
                view.xui = _windowGroup.xui;
                XUiFromXmlReversePatch.setController(_node, view, _parent);
                view.SetDefaults(_parent);
                XUiFromXmlReversePatch.parseAttributes(_node, view, _parent, _controlParams);
                view.SetPostParsingDefaults(_parent);

                view.Controller.WindowGroup = _windowGroup;
                createScrollBarViewComponents(_node, view as XUiV_ScrollBar, _windowGroup, _controlParams);
                __result = view;
                return false;
        }

        if(view != null)
        {
            view.xui = _windowGroup.xui;
            XUiFromXmlReversePatch.setController(_node, view, _parent);
            view.SetDefaults(_parent);
            XUiFromXmlReversePatch.parseAttributes(_node, view, _parent, _controlParams);
            view.SetPostParsingDefaults(_parent);

            view.Controller.WindowGroup = _windowGroup;

            foreach (XElement childNode in _node.Elements())
            {
                XUiFromXmlReversePatch.parseViewComponents(childNode, _windowGroup, view.Controller, _controlParams: _controlParams);
            }
            
            __result = view;

            return false;
        }

        return true;
    }

    private static void createScrollBarViewComponents(XElement _node, XUiV_ScrollBar view, XUiWindowGroup _windowGroup, Dictionary<string, object> _controlParams = null)
    {
        if (!view.HasXMLChildren)
        {
            return;
        }

        int childCount = _node.Elements().Count<XElement>();
       
        Logging.Out(TAG, "Child Count = " + childCount);

        if (childCount > 2)
        {
            XUiFromXmlReversePatch.logForNode(LogType.Log, _node, "[XUi] XUiFromXml::parseByElementName: Invalid scrollbar child count. Must have zero to two child element.");
        }
        else
        {
            foreach (XElement child in _node.Elements())
            {
                Logging.Out(TAG, "Creating ScrollBar View Component");
                ParseScrollBarViewComponents(child, view.Controller, _windowGroup, _controlParams: _controlParams);
            }
        }
    }

    private static void ParseScrollBarViewComponents(XElement node, XUiController parent, XUiWindowGroup windowGroup,
        string nodeNameOverride = "", Dictionary<string, object> _controlParams = null)
    {
        string name = node.Name.LocalName;
        string id = name;

        if (nodeNameOverride == "" && node.HasAttribute("name"))
        {
            id = node.GetAttribute("name");
        }
        else if (nodeNameOverride != "")
        {
            id = nodeNameOverride;
        }

        if (_controlParams != null)
        {
            XUiFromXmlReversePatch.parseControlParams(node, _controlParams);
        }

        XUiView view = null;

        switch (name)
        {
            case "sprite":
                view = new XUiC_Scrollbar_Sprite(id);
                Logging.Out(TAG, "ScrollBar Sprite Created");
                break;
            case "button":
                view = new XUiC_ScrollBar_Button(id);
                Logging.Out(TAG, "ScrollBar Button Created");
                break;
        }

        if (view != null)
        {
            view.xui = windowGroup.xui;
            XUiFromXmlReversePatch.setController(node, view, parent);
            view.SetDefaults(parent);
            XUiFromXmlReversePatch.parseAttributes(node, view, parent, _controlParams);
            view.SetPostParsingDefaults(parent);

            view.Controller.WindowGroup = windowGroup;
        }
    }
}

[HarmonyPatch(typeof(XUiFromXml))]
public class XUiFromXmlReversePatch
{
    private const string TAG = "Error Reverse Patching XUiFromXML method: ";

    [HarmonyReversePatch]
    [HarmonyPatch("parseControlParams")]
    public static void parseControlParams(XElement _node, Dictionary<string, object> _controlParams)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "parseControlParams");
    }

    [HarmonyReversePatch]
    [HarmonyPatch("setController")]
    public static void setController(XElement _node, XUiView _viewComponent, XUiController _parent)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "parseController");
    }

    [HarmonyReversePatch]
    [HarmonyPatch("parseAttributes")]
    public static void parseAttributes(XElement _node, XUiView _viewComponent, XUiController _parent,
        Dictionary<string, object> _controlParams = null)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "parseAttributes");
    }

    [HarmonyReversePatch]
    [HarmonyPatch("parseViewComponents")]
    public static XUiView parseViewComponents(XElement _node, XUiWindowGroup _windowGroup, XUiController _parent = null, 
        string nodeNameOverride = "", Dictionary<string, object> _controlParams = null)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "parseViewComponents");
    }

    [HarmonyReversePatch]
    [HarmonyPatch("logForNode")]
    public static void logForNode(LogType _level, XElement _node, string _message)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "logForNode");
    }
}

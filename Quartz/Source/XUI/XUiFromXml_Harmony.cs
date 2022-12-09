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
using Quartz.Managers;
using Quartz.Views;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static XUiFromXml;

[HarmonyPatch(typeof(XUiFromXml))]
public class XUiFromXmlPatch
{
    private const string TAG = "XUiFromXmlPatch";
    private static bool FontsLoaded = false;

    [HarmonyPrefix]
    [HarmonyPatch("parseByElementName")]
    public static bool parseByElementName(ref XUiView __result,
        XmlNode _node, XUiController _parent, XUiWindowGroup _windowGroup,
        string nodeNameOverride = "", Dictionary<string, object> _controlParams = null)
    {
        XmlElement xmlElement = (XmlElement)_node;
        string name = xmlElement.Name;
        string id = name;

        if (nodeNameOverride == "" && xmlElement.HasAttribute("name"))
        {
            id = xmlElement.GetAttribute("name");
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

        switch(name)
        {
            case "curvedlabel":
                view = new CurvedLabel(id);
                break;
            case "videoplayer":
                view = new XUiV_VideoPlayer(id);
                break;
            case "scrollview":
                view = new ScrollViewContainer(id);
                break;
            case "scrollbar":
                view = new ScrollBarView(id);
                view.xui = _windowGroup.xui;
                XUiFromXmlReversePatch.parseController(_node, view, _parent);
                XUiFromXmlReversePatch.parseAttributes(_node, view, _parent, _controlParams);

                view.Controller.WindowGroup = _windowGroup;
                createScrollBarViewComponents(_node, view as ScrollBarView, _windowGroup, _controlParams);
                __result = view;
                return false;
        }

        if(view != null)
        {
            view.xui = _windowGroup.xui;
            XUiFromXmlReversePatch.parseController(_node, view, _parent);
            XUiFromXmlReversePatch.parseAttributes(_node, view, _parent, _controlParams);

            view.Controller.WindowGroup = _windowGroup;

            foreach (XmlNode childNode in _node.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                    XUiFromXmlReversePatch.parseViewComponents(childNode, _windowGroup, view.Controller, _controlParams: _controlParams);
            }
            
            __result = view;

            return false;
        }

        return true;
    }

    private static void createScrollBarViewComponents(XmlNode _node, ScrollBarView view, XUiWindowGroup _windowGroup, Dictionary<string, object> _controlParams = null)
    {
        if (!view.HasXMLChildren)
        {
            return;
        }

        int childCount = 0;
        XmlNode[] childNodes = new XmlNode[2];
        foreach(XmlNode childNode in _node.ChildNodes)
        {
            if (childNode.NodeType == XmlNodeType.Element)
            {
                childCount++;
                if (childCount == 1 || childCount == 2)
                {
                    childNodes[childCount-1] = childNode;
                }
            }
        }

        Logging.Out(TAG, "Child Count = " + childCount);

        if (childCount > 2)
        {
            XUiFromXmlReversePatch.logForNode(LogType.Log, _node, "[XUi] XUiFromXml::parseByElementName: Invalid scrollbar child count. Must have zero to two child element.");
        }
        else
        {
            for (int i = 0; i < childCount; i++)
            {
                Logging.Out(TAG, "Creating ScrollBar View Component");
                ParseScrollBarViewComponents(childNodes[i], view.Controller, _windowGroup, _controlParams: _controlParams);
            }
        }
    }

    private static void ParseScrollBarViewComponents(XmlNode node, XUiController parent, XUiWindowGroup windowGroup,
        string nodeNameOverride = "", Dictionary<string, object> _controlParams = null)
    {
        XmlElement xmlElement = (XmlElement)node;
        string name = xmlElement.Name;
        string id = name;

        if (nodeNameOverride == "" && xmlElement.HasAttribute("name"))
        {
            id = xmlElement.GetAttribute("name");
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
                view = new Scrollbar_Sprite(id);
                Logging.Out(TAG, "ScrollBar Sprite Created");
                break;
            case "button":
                view = new ScrollBar_Button(id);
                Logging.Out(TAG, "ScrollBar Button Created");
                break;
        }

        if (view != null)
        {
            view.xui = windowGroup.xui;
            XUiFromXmlReversePatch.parseController(node, view, parent);
            XUiFromXmlReversePatch.parseAttributes(node, view, parent, _controlParams);

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
    public static void parseControlParams(XmlNode _node, Dictionary<string, object> _controlParams)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "parseControlParams");
    }

    [HarmonyReversePatch]
    [HarmonyPatch("parseController")]
    public static void parseController(XmlNode _node, XUiView _viewComponent, XUiController _parent)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "parseController");
    }

    [HarmonyReversePatch]
    [HarmonyPatch("parseAttributes")]
    public static void parseAttributes(XmlNode _node, XUiView _viewComponent, XUiController _parent,
        Dictionary<string, object> _controlParams = null)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "parseAttributes");
    }

    [HarmonyReversePatch]
    [HarmonyPatch("parseViewComponents")]
    public static XUiView parseViewComponents(XmlNode _node, XUiWindowGroup _windowGroup, XUiController _parent = null, 
        string nodeNameOverride = "", Dictionary<string, object> _controlParams = null)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "parseViewComponents");
    }

    [HarmonyReversePatch]
    [HarmonyPatch("logForNode")]
    public static void logForNode(LogType _level, XmlNode _node, string _message)
    {
        // its a stub so it has no initial content
        throw new NotImplementedException(TAG + "logForNode");
    }
}

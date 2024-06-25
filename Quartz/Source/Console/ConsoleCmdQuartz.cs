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

using Quartz.Debugging;
using Quartz.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleCmdQuartz : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get
        {
            return true;
        }
    }

    public override bool AllowedInMainMenu
    {
        get
        {
            return true;
        }
    }

    public ConsoleCmdQuartz()
    {
    }

    public override string[] getCommands()
    {
        return new string[]
        {
            "quartz"
        };
    }

    public override string getDescription()
    {
        return "Execute Quartz operations";
    }

    public override string GetHelp()
    {
        return "Usage:\n   " +
            "quartz debug \n" +
            "quartz uiatlas - opens the UIAtlas window \n";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        if (_params.Count < 1)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No sub command given.");
            return;
        }

        switch(_params[0].ToLowerInvariant())
        {
            case "debug":
                ExecuteDebug(_params);
                break;
            case "uiatlas":
                ExecuteUiAtlas(_params);
                break;
            default:
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
                break;

        }
    }

    private void ExecuteUiAtlas(List<string> _params)
    {
        if (_params.Count > 1)
        {
            SdtdConsole.Instance.Output("Wrong number of arguments, expected 1, found " + _params.Count.ToString() + ".");
            return;
        }

        XUi xuiInstance = getXuiInstance();
        if (xuiInstance == null)
        {
            return;
        }

        xuiInstance.playerUI.windowManager.CloseIfOpen(XUiC_MainMenu.ID);
        xuiInstance.playerUI.windowManager.OpenIfNotOpen(XUiC_UiAtlasList.ID, true, false, false);
    }

    private void ExecuteDebug(List<string> _params)
    {
        if (_params.Count > 3)
        {
            SdtdConsole.Instance.Output("Wrong number of arguments, expected 1 - 3, found " + _params.Count.ToString() + ".");
            return;
        }

        if(_params.Count == 1)
        {
            EnableDebug(_params);
            return;
        }

        switch(_params[1])
        {
            case "fontSize":
            case "font_size":
                SetFontSize(_params);
                break;
            case "fontColor":
            case "font_color":
                SetFontColor(_params);
                break;
            default:
                EnableDebug(_params);
                break;
        }
    }

    private void EnableDebug(List<string> _params)
    {
        bool enable = !DebuggingSettings.IsDebugEnabled;
        if (_params.Count == 2)
        {
            if (!ParseBool(_params[1], ref enable))
            {
                SdtdConsole.Instance.Output("Second argument not recognize");
                return;
            }
        }

        if (enable)
        {
            DebuggingSettings.EnableDebugging();
        }
        else
        {
            DebuggingSettings.DisableDebugging();
        }

        SdtdConsole.Instance.Output("Quartz Debug " + (DebuggingSettings.IsDebugEnabled ? "on" : "off"));
    }

    private bool ParseBool(string value, ref bool _return)
    {
        if (value == null)
        {
            return false;
        }

        if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) || "On".Equals(value, StringComparison.OrdinalIgnoreCase) || "1".Equals(value, StringComparison.OrdinalIgnoreCase))
        {
            _return = true;
            return true;
        }

        if ("False".Equals(value, StringComparison.OrdinalIgnoreCase) || "Off".Equals(value, StringComparison.OrdinalIgnoreCase) || "0".Equals(value, StringComparison.OrdinalIgnoreCase))
        {
            _return = false;
            return true;
        }

        return false;
    }

    private void SetFontSize(List<string> _params)
    {
        int fontSize = 0;
        if (!int.TryParse(_params[2],out fontSize)){
            SdtdConsole.Instance.Output("Third argument not recognize");
            return;
        }

        DebuggingSettings.SetFontSize(fontSize);
        SdtdConsole.Instance.Output("Quartz Debug FontSize set to " + fontSize);
    }

    private void SetFontColor(List<string> _params)
    {
        string fontColor = _params[2];
        Color color = Color.white;
        switch (fontColor.ToLower())
        {
            case "red":
                color = Color.red;
                break;
            case "green":
                color = Color.green;
                break;
            case "blue":
                color = Color.blue;
                break;
            case "white":
                color = Color.white;
                break;
            case "black":
                color = Color.black;
                break;
            case "yellow":
                color = Color.yellow;
                break;
            case "cyan":
                color = Color.cyan;
                break;
            case "magenta":
                color = Color.blue;
                break;
            case "gray":
            case "grey":
                color = Color.grey;
                break;
            default:
                SdtdConsole.Instance.Output("Third argument not recognize");
                return;
        }
        QuartzDebug.FontColor = color;
        SdtdConsole.Instance.Output("Quartz Debug FontColor set to " + fontColor);
    }


    private XUi getXuiInstance()
    {
        XUi[] array = UnityEngine.Object.FindObjectsOfType<XUi>();
        Array.Sort<XUi>(array, (XUi _x, XUi _y) => string.Compare(_x.playerUI.windowManager.gameObject.name, _y.playerUI.windowManager.gameObject.name, StringComparison.Ordinal));
        return array[0];
    }
}


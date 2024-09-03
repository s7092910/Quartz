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

[HarmonyPatch(typeof(XUiC_LootWindow))]
public class XUiC_LootWindowPatch
{
    private const string TAG = "XUiC_LootWindowPatch";

    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    public static bool Update(XUiC_LootWindow __instance, float _dt,
        ref bool ___activeKeyDown, ref bool ___wasReleased, ref bool ___isClosing,
        ref XUiC_ContainerStandardControls ___controls, 
        ref XUiWindowGroup ___windowGroup)
    {
        XUiControllerPatch.Update(__instance, _dt);

        if (___windowGroup.isShowing)
        {
            if (!__instance.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
            {
                ___wasReleased = true;
            }

            if (___wasReleased)
            {
                if (__instance.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
                {
                    ___activeKeyDown = true;
                }

                if (__instance.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && ___activeKeyDown && !__instance.xui.playerUI.windowManager.IsInputActive())
                {
                    ___activeKeyDown = false;
                    __instance.xui.playerUI.windowManager.CloseAllOpenWindows();
                }
            }
        }
        if (GameManager.Instance == null && GameManager.Instance.World == null)
        {
            return false;
        }

        if (!___isClosing && __instance.ViewComponent != null && __instance.ViewComponent.IsVisible && !__instance.xui.playerUI.windowManager.IsInputActive()
            && (__instance.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || __instance.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
        {
            if (___controls is Quartz.XUiC_ContainerStandardControls controls)
            {
                controls.MoveAllButLocked();
            }
            else
            {
                ___controls.MoveAll();
            }
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("GetBindingValue")]
    public static bool GetBindingValue(XUiC_LootWindow __instance, ref bool __result, ref string _value, string _bindingName)
    {
        switch (_bindingName)
        {
            case "userlockmode":
                _value = __instance.lootContainer != null && __instance.lootContainer is Quartz.XUiC_LootContainer lootContainer ? lootContainer.IsIndividualSlotLockingAllowed.ToString() : "False";
                __result = true;
                return false;
            default:
                return true;
        }
    }
}

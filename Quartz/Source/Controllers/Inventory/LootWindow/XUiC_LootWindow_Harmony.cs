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
using UnityEngine;

[HarmonyPatch(typeof(XUiC_LootWindow))]
public class XUiC_LootWindowPatch
{
    private const string TAG = "XUiC_LootWindowPatch";

    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    public static bool Update(XUiC_LootWindow __instance, float _dt)
    {
        XUiControllerPatch.Update(__instance, _dt);

        if (__instance.windowGroup.isShowing)
        {
            if (!__instance.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
            {
                __instance.wasReleased = true;
            }

            if (__instance.wasReleased)
            {
                if (__instance.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
                {
                    __instance.activeKeyDown = true;
                }

                if (__instance.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && __instance.activeKeyDown && !__instance.xui.playerUI.windowManager.IsInputActive())
                {
                    __instance.activeKeyDown = false;
                    __instance.xui.playerUI.windowManager.CloseAllOpenWindows();
                }
            }
        }
        if (__instance.te != null)
        {
            Vector3 vector = __instance.te.ToWorldCenterPos();
            if (vector != Vector3.zero)
            {
                float num = Constants.cCollectItemDistance + 30f;
                float sqrMagnitude = (__instance.xui.playerUI.entityPlayer.position - vector).sqrMagnitude;
                if (sqrMagnitude > num * num)
                {
                    Log.Out("Loot Window closed at distance {0}", new object[] { Mathf.Sqrt(sqrMagnitude) });
                    __instance.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
                    __instance.CloseContainer(false);
                }
            }
        }
        if (GameManager.Instance == null && GameManager.Instance.World == null)
        {
            return false;
        }

        if (!__instance.isClosing && __instance.ViewComponent != null && __instance.ViewComponent.IsVisible && !__instance.xui.playerUI.windowManager.IsInputActive()
            && (__instance.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || __instance.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
        {
            if (__instance.standardControls is Quartz.XUiC_ContainerStandardControls controls)
            {
                controls.MoveAllButLocked();
            }
            else
            {
                __instance.standardControls.MoveAll();
            }
        }

        return false;
    }
}

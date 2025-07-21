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

using Audio;
using HarmonyLib;
using Quartz;
using System.Collections.Generic;
using UnityEngine;

[HarmonyPatch(typeof(XUiC_MapArea))]
public class XUiC_MapAreaPatch
{
    private const string TAG = "XUiC_MapAreaPatch";

    [HarmonyPrefix]
    [HarmonyPatch("initMap")]
    private static bool initMap(XUiC_MapArea __instance)
    {
        if (__instance.xui.playerUI.entityPlayer == null)
        {
            return false;
        }
        __instance.localPlayer = __instance.xui.playerUI.entityPlayer;
        __instance.bMapInitialized = true;
        __instance.xuiTexture.Texture = __instance.mapTexture;
        __instance.cTexMiddle = __instance.xuiTexture.Size / 2;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("OnPreRender")]
    private static bool OnPreRender(LocalPlayerCamera _localPlayerCamera, XUiV_Texture ___xuiTexture, float ___mapScale,
        Vector2 ___mapBGPos, Vector2 ___mapPos)
    {
        float xScale = ___xuiTexture.Size.x / 712f;
        float yScale = ___xuiTexture.Size.y / 712f;
        Shader.SetGlobalVector("_MainMapPosAndScale", new Vector4(___mapPos.x, ___mapPos.y, ___mapScale * xScale, ___mapScale * yScale));
        Shader.SetGlobalVector("_MainMapBGPosAndScale", new Vector4(___mapBGPos.x, ___mapBGPos.y, ___mapScale * xScale, ___mapScale * yScale));

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("positionMap")]
    private static bool positionMap(XUiV_Texture ___xuiTexture, ref float ___mapScale, ref float ___zoomScale,
        ref Vector2 ___mapBGPos, ref Vector2 ___mapPos, Vector2 ___mapMiddlePosPixel, Vector2 ___mapScrollTextureOffset, Vector2 ___mapMiddlePosChunks)
    {

        float xScale = ___xuiTexture.Size.x / 712f;
        float yScale = ___xuiTexture.Size.y / 712f;

        float numX = (2048f - (336f * xScale) * ___zoomScale) / 2f;
        float numY = (2048f - (336f * yScale) * ___zoomScale) / 2f;
        ___mapScale = 336f * ___zoomScale / 2048f;
        float num2 = (numX + (___mapMiddlePosPixel.x - ___mapMiddlePosChunks.x)) / 2048f;
        float num3 = (numY + (___mapMiddlePosPixel.y - ___mapMiddlePosChunks.y)) / 2048f;
        ___mapPos = new Vector3(num2 + ___mapScrollTextureOffset.x, num3 + ___mapScrollTextureOffset.y, 0f);
        ___mapBGPos.x = (numX + ___mapMiddlePosPixel.x) / 2048f;
        ___mapBGPos.y = (numY + ___mapMiddlePosPixel.y) / 2048f;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("onMapScrolled")]
    private static bool onMapScrolled(XUiController _sender, float _delta, XUiC_MapArea __instance, XUiV_Texture ___xuiTexture,
        ref float ___zoomScale, ref float ___targetZoomScale)
    {
        float x = ___xuiTexture.Size.x / 712f;
        float y = ___xuiTexture.Size.y / 712f;

        float sizeMax = Mathf.Max(x, y);

        float zoomMultiplier = 6f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            zoomMultiplier = 5f * ___zoomScale;
        }
        float min = 0.7f;
        float max = 6.15f / sizeMax; //Might change to 5.95f to fix weird border in texture 
        ___targetZoomScale = Utils.FastClamp(___zoomScale - _delta * zoomMultiplier, min, max);
        if (_delta < 0f)
        {
            Manager.PlayInsidePlayerHead("map_zoom_in", -1, 0f, false);
        }
        else
        {
            Manager.PlayInsidePlayerHead("map_zoom_out", -1, 0f, false);
        }
        __instance.closeAllPopups();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("mousePosToWindowPos")]
    private static bool mousePosToWindowPos(ref Vector3 __result, XUiC_MapArea __instance, XUiV_Texture ___xuiTexture, Vector3 _mousePos)
    {
        int x = ___xuiTexture.Position.x;
        float y = ___xuiTexture.Position.y;
        Vector2i mouseXUIPosition = __instance.xui.GetMouseXUIPosition();
        Vector3 windowPos = new Vector3(mouseXUIPosition.x, mouseXUIPosition.y, 0f);
        windowPos.x += 217f - x;
        windowPos.y -= 362f + y;
        windowPos.y = -windowPos.y;
        __result = windowPos * 0.9493333f;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("screenPosToWorldPos")]
    private static bool screenPosToWorldPos(ref Vector3 __result, XUiC_MapArea __instance, Vector3 _mousePos,
        XUiV_Texture ___xuiTexture, Vector2 ___mapMiddlePosPixel, float ___zoomScale,
        bool needY = false)
    {
        float xScale = ___xuiTexture.Size.x / 712f;
        float zScale = ___xuiTexture.Size.y / 712f;

        Vector3 mousePos = _mousePos;
        Vector3 textureScreenPosition = __instance.xui.playerUI.camera.WorldToScreenPoint(___xuiTexture.UiTransform.position);
        mousePos.x -= textureScreenPosition.x;
        mousePos.y -= textureScreenPosition.y;
        mousePos.y *= -1f;
        Bounds xUIWindowScreenBounds = __instance.xui.GetXUIWindowScreenBounds(___xuiTexture.UiTransform, false);
        Vector3 mapScreenBounds = xUIWindowScreenBounds.max - xUIWindowScreenBounds.min;

        float blockScaleX = mapScreenBounds.x / (336f * xScale);
        float blockScaleY = mapScreenBounds.y / (336f * zScale);
        float x = (mousePos.x - mapScreenBounds.x / 2f) / blockScaleX * ___zoomScale + ___mapMiddlePosPixel.x;
        float z = -(mousePos.y - mapScreenBounds.y / 2f) / blockScaleY * ___zoomScale + ___mapMiddlePosPixel.y;
        float y = 0f;
        if (needY)
        {
            y = GameManager.Instance.World.GetHeightAt(x, z);
        }
        __result = new Vector3(x, y, z);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("updateNavObjectList")]
    public static bool updateNavObjectList(XUiC_MapArea __instance)
    {
        bool flag = true;
        bool flag2 = false;
        List<NavObject> navObjectList = NavObjectManager.Instance.NavObjectList;
        __instance.navObjectsOnMapAlive.Clear();
        for (int i = 0; i < navObjectList.Count; i++)
        {
            NavObject navObject = navObjectList[i];
            NavObjectMapSettings currentMapSettings = navObject.CurrentMapSettings;
            if (currentMapSettings != null && navObject.IsOnMap())
            {
                int key = navObject.Key;
                GameObject gameObject;
                UISprite uisprite;
                if (!__instance.keyToNavObject.ContainsKey(key))
                {
                    gameObject = __instance.transformSpritesParent.gameObject.AddChild(__instance.prefabMapSprite);
                    uisprite = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
                    string spriteName = navObject.GetSpriteName(currentMapSettings);
                    uisprite.atlas = __instance.xui.GetAtlasByName(((global::UnityEngine.Object)uisprite.atlas).name, spriteName);
                    uisprite.spriteName = spriteName;
                    uisprite.depth = currentMapSettings.Layer;
                    __instance.keyToNavObject[key] = navObject;
                    __instance.keyToNavSprite[key] = gameObject;
                }
                else
                {
                    gameObject = __instance.keyToNavSprite[key];
                }
                EntityPlayer entityPlayer = navObject.TrackedEntity as EntityPlayer;
                string text = ((entityPlayer != null) ? entityPlayer.PlayerDisplayName : navObject.DisplayName);
                if (!string.IsNullOrEmpty(text))
                {
                    UILabel component = gameObject.transform.Find("Name").GetComponent<UILabel>();
                    component.text = text;
                    component.font = __instance.xui.GetUIFontByName("ReferenceFont", true);
                    component.gameObject.SetActive(true);
                    component.color = (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color);
                }
                else
                {
                    gameObject.transform.Find("Name").GetComponent<UILabel>().text = "";
                }
                float spriteZoomScaleFac = __instance.getSpriteZoomScaleFac();
                uisprite = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
                Vector3 vector = currentMapSettings.IconScaleVector * spriteZoomScaleFac;
                uisprite.width = Mathf.Clamp((int)((float)__instance.cSpriteScale * vector.x), 9, 100);
                uisprite.height = Mathf.Clamp((int)((float)__instance.cSpriteScale * vector.y), 9, 100);
                uisprite.color = (navObject.hiddenOnCompass ? Color.grey : (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color));
                uisprite.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, -navObject.Rotation.y);
                gameObject.transform.localPosition = __instance.worldPosToScreenPos(navObject.GetPosition() + Origin.position);
                if (currentMapSettings.AdjustCenter)
                {
                    gameObject.transform.localPosition += new Vector3((float)(uisprite.width / 2), (float)(uisprite.height / 2), 0f);
                }
                __instance.navObjectsOnMapAlive.Add((long)key);
            }
        }
        if (flag && !flag2 && __instance.bMapCursorSet)
        {
            __instance.SetMapCursor(false);
            __instance.xui.currentToolTip.ToolTip = string.Empty;
        }

        return false;
    }
}

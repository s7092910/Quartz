﻿/*Copyright 2022 Christopher Beda

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

[HarmonyPatch(typeof(UIScrollView))]
public class UIScrollViewPatch
{
    private const string TAG = "UIScrollView";

    //[HarmonyPrefix]
    //[HarmonyPatch("ResetPosition")]
    //public static bool ResetPosition(UIScrollView __instance, ref bool ___mCalculatedBounds, UIWidget.Pivot ___contentPivot)
    //{
    //    if (NGUITools.GetActive(__instance))
    //    {
    //        ___mCalculatedBounds = false;
    //        Vector2 pivotOffset = NGUIMath.GetPivotOffset(___contentPivot);
    //        __instance.SetDragAmount(pivotOffset.x, 1f - pivotOffset.y, updateScrollbars: false);
    //        __instance.SetDragAmount(pivotOffset.x, 1f - pivotOffset.y, updateScrollbars: true);
    //    }

    //    return false;
    //}

    [HarmonyPostfix]
    [HarmonyPatch("OnDisable")]
    public static void ResetPosition(UIScrollView __instance)
    {
        if(__instance is Quartz.UIScrollView scrollview)
        {
            scrollview.IsOpen = false;
        }
    }
}

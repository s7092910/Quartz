/*Copyright 2024 Christopher Beda

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
using InControl;
using Quartz;
using Quartz.Map;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[HarmonyPatch(typeof(MapChunkDatabase))]
public class MapChunkDatabasePatch
{
    private const string TAG = "MapChunkDatabasePatch";

    [HarmonyPostfix]
    [HarmonyPatch("Add")]
    [HarmonyPatch(new Type[] { typeof(List<int>), typeof(List<ushort[]>)})]
    private static void Add(MapChunkDatabase __instance, List<int> _chunks, List<ushort[]> _mapPieces)
    {
        Logging.Inform(TAG, "Add(List<int>, List<ushort[]>) called");
        for (int i = 0; i < _chunks.Count; i++)
        {
            MapColorDatabase.AddPackedMapColors(_chunks[i], _mapPieces[i]);
        }
    }
}

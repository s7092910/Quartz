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
using Quartz.Map;
using System;

[HarmonyPatch(typeof(ChunkCluster))]
public class ChunkCluster_Harmony
{

    [HarmonyPostfix]
    [HarmonyPatch("SetBlock")]
    [HarmonyPatch(new Type[] { typeof(Vector3i), typeof(bool), typeof(BlockValue), typeof(bool), typeof(sbyte), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int) })]
    public static void SetBlock(ChunkCluster __instance, Vector3i _pos, bool _isChangeBV, BlockValue _bv, bool _isChangeDensity, sbyte _density, bool _isNotify, bool _isUpdateLight, bool _isForceDensity, bool _wasChild, int _changedByEntityId)
    {
        int x = World.toChunkXZ(_pos.x);
        int z = World.toChunkXZ(_pos.z);

        Chunk chunk = __instance.GetChunkSync(x, z);

        if (chunk != null && !chunk.NeedsDecoration)
        {
            int key = IMapChunkDatabase.ToChunkDBKey(x , z);
            MapColorDatabase.AddPackedMapColors(key, chunk.GetMapColors());
        }
    }
}

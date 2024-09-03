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

using System.Collections.Generic;

namespace Quartz.Map
{
    public static class MapColorDatabase
    {
        public static Dictionary<long, uint[]> mapDataCache = new Dictionary<long, uint[]>();

        public static uint[] GetPackedMapColors(this IMapChunkDatabase database, int x, int y)
        {
            long key = WorldChunkCache.MakeChunkKey(x, y);
            if (!mapDataCache.TryGetValue(key, out uint[] cachedChunk))
            {
                ushort[] mapColors = database.GetMapColors(key);
                if (mapColors == null)
                {
                    return null;
                }

                cachedChunk = AddPackedMapColors(key, mapColors);
            }

            return cachedChunk;
        }

        public static uint[] AddPackedMapColors(long key, ushort[] mapColors)
        {
            uint[] mapData = new uint[128];
            int textureOffset = 0;
            for (int i = 0; i < 128; i++, textureOffset += 2)
            {
                mapData[i] = PackShorts(mapColors[textureOffset], mapColors[textureOffset + 1]);
            }

            mapDataCache[key] = mapData;

            return mapData;
        }

        public static uint PackShorts(ushort first, ushort second)
        {
            return ((uint)first << 16) + second;
        }
    }
}

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

using UnityEngine.Experimental.Rendering;
using UnityEngine;

namespace Quartz.Utils
{
    public static class TextureExtensions
    {
        public static Texture2D TextureFromGPU(this Texture src)
        {
            if (src == null || src.isReadable)
            {
                return null;
            }

            GraphicsFormat fmt = SystemInfo.GetCompatibleFormat(src.graphicsFormat, FormatUsage.Render);

            RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 32, fmt);
            RenderTexture active = RenderTexture.active;

            Graphics.Blit(src, rt);

            Texture2D tex = new Texture2D(src.width, src.height, fmt, src.mipmapCount, TextureCreationFlags.MipChain);

            tex.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0, true);
            tex.Apply(false, false);


            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(rt);

            return tex;
        }
    }
}

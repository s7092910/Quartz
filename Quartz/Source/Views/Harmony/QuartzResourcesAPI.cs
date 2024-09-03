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
using UnityEngine;

namespace Quartz.Source.Views.Harmony
{
    internal class QuartzResourcesAPI : ResourcesAPI
    {
        
        private Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

        public QuartzResourcesAPI() { 
            ResourcesAPI.overrideAPI = this;
        }

        protected override Shader FindShaderByName(string name)
        {
            Shader shader = base.FindShaderByName(name);
            if (shader != null)
            {
                return shader;
            }

            if (!shaders.TryGetValue(name, out shader))
            {
                switch (name)
                {
                    case "Unlit/MaskedMinimap":
                        shader = DataLoader.LoadAsset<Shader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/MaskedMinimap.shader");
                        shaders.Add(name, shader);
                        break;
                    case "Unlit/Transparent FixableMask":
                        shader = DataLoader.LoadAsset<Shader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/UnlitTransparentFixableMask.shader");
                        shaders.Add(name, shader);
                        break;
                }
            }

            return shader;
        }
    }
}

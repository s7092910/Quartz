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

using System;

namespace Quartz
{
    public class XUiC_RandomTexture : XUiController
    {
        private const string TAG = "RandomTexture";

        private const char SPLITTER = ',';
        private string[] textures;
        private readonly Random rnd = new Random();

        public override void OnOpen()
        {
            base.OnOpen();
            RefreshBindings();
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "randomtexture":
                    value = getRandomTexture();
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        public override bool ParseAttribute(string attribute, string value, XUiController parent)
        {
            if (attribute != null)
            {
                switch (attribute)
                {
                    case "textures":
                        parseTextures(value);
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, parent);
                }
            }
            return false;
        }

        private void parseTextures(string texturesString)
        {
            if (string.IsNullOrEmpty(texturesString))
            {
                return;
            }

            textures = texturesString.Split(SPLITTER);
        }

        private string getRandomTexture()
        {
            if (textures == null)
            {
                return string.Empty;
            }
            string textureName = textures[rnd.Next(textures.Length)];
            textureName = textureName.Trim();
            Logging.Out(TAG, "Random Texture: " + textureName);
            return textureName;
        }
    }
}


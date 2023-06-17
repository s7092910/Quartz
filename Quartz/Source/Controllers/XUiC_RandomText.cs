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
    public class XUiC_RandomText : XUiController
    {
        private const string TAG = "RandomText";

        private const char SPLITTER = ',';
        private string[] texts;
        private readonly Random rnd = new Random();

        public override void OnOpen()
        {
            base.OnOpen();
            RefreshBindings();
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch( bindingName )
            {
                case "randomtext":
                    value = Localization.Get(getRandomText());
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
                    case "texts":
                        parseTexts(value);
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, parent);
                }
            }
            return false;
        }

        private void parseTexts(string textsString)
        {
            if (string.IsNullOrEmpty(textsString))
            {
                return;
            }

            texts = textsString.Split(SPLITTER);
        }

        private string getRandomText()
        {
            if(texts == null)
            {
                return string.Empty;
            }
            string text = texts[rnd.Next(texts.Length)];
            text = text.Trim();
            Logging.Out(TAG, "Random Text: " + text);
            return text;
        }
    }
}

/*Copyright 2023 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using UnityEngine;

namespace Quartz
{
    public class XUiC_RecipeEntry : global::XUiC_RecipeEntry
    {
        private XUiV_Sprite background;

        private Color32 selectedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        private Color32 nonSelectedColor = new Color32(64, 64, 64, byte.MaxValue);

        private string selectedBackground = "ui_game_select_row";
        private string nonSelectedBackground = "menu_empty";

        public override void Init()
        {
            base.Init();
            XUiController backgroundController = GetChildById("background");
            if(backgroundController != null )
            {
                background = backgroundController.ViewComponent as XUiV_Sprite;
            }
        }
        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch(bindingName)
            {
                case "workstationname":
                    value = Recipe != null ? Localization.Get(Recipe.craftingArea): "";
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        public override bool ParseAttribute(string name, string value, XUiController parent)
        {
            switch(name)
            {
                case "selectedcolor":
                    selectedColor = StringParsers.ParseColor32(value);
                    return true;
                case "nonselectedcolor":
                    nonSelectedColor = StringParsers.ParseColor32(value);
                    return true;
                case "selectedbackground":
                    selectedBackground = value;
                    return true;
                case "nonselectedbackground":
                    nonSelectedBackground = value;
                    return true;
                default:
                    return base.ParseAttribute(name, value, parent);
            }
           
        }

        protected override void SelectedChanged(bool isSelected)
        {
            if (background != null)
            {
                background.Color = isSelected ? selectedColor : nonSelectedColor;
                background.SpriteName = isSelected ? selectedBackground : nonSelectedBackground;
            }
        }
    }
}

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

using UnityEngine;

namespace Quartz
{
    public class XUiC_ItemStack : global::XUiC_ItemStack
    {
        private const string TAG = "ItemStack";

        private bool matchesSearch;
        private bool isSearchActive;

        protected Color32 lockedSlotColor = new Color32(96, 96, 96, byte.MaxValue);
        protected Color32 searchColor = new Color32(96, 96, 96, byte.MaxValue);
        protected Color32 noMatchTintColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        protected bool isLockedSlotColorSet;
        protected bool isSearchColorSet;
        protected bool isNoMatchTintColorSet;

        protected new Color32 selectionBorderColor;

        private readonly CachedStringFormatterXuiRgbaColor colorFormatter = new CachedStringFormatterXuiRgbaColor();

        public bool IsSearchActive
        {
            get
            {
                return isSearchActive;
            }
            set
            {
                if (isSearchActive != value)
                {
                    isSearchActive = value;
                    RefreshBindings(false);
                }
            }
        }

        public bool MatchesSearch
        {
            get
            {
                return matchesSearch;
            }
            set
            {
                if (matchesSearch != value)
                {
                    matchesSearch = value;
                    RefreshBindings(false);
                }
            }
        }

        public override string ItemIconColor
        {
            get
            {
                if (!isSearchActive || matchesSearch || !isNoMatchTintColorSet)
                {
                    return base.ItemIconColor;
                }

                return itemiconcolorFormatter.Format(noMatchTintColor);
            }
        }

        public override void Update(float _dt)
        {
            updateBorderColor();
            base.Update(_dt);
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "issearchactive":
                    value = isSearchActive.ToString();
                    return true;
                case "matchessearch":
                    value = matchesSearch.ToString();
                    return true;
                case "selectionbordercolor":
                    value = colorFormatter.Format(SelectionBorderColor);
                    return true;
                case "itemql":
                    if (itemClass == null || !ShowDurability)
                    {
                        value = "";
                        return true;
                    }

                    value = ((itemStack.itemValue.Quality > 0) ? itemcountFormatter.Format(itemStack.itemValue.Quality) : (itemStack.itemValue.IsMod ? "*" : ""));
                    return true;
                case "stackcount":
                    if (itemClass == null || ShowDurability)
                    {
                        value = "";
                        return true;
                    }

                    value = ((itemClass.Stacknumber == 1) ? "" : itemcountFormatter.Format(itemStack.count));
                    return true;
                case "durabilitycolor":
                    Color32 color = QualityInfo.GetQualityColor(itemStack?.itemValue.Quality ?? 0);
                    if (isSearchActive && !matchesSearch)
                    {
                        color = color.Over(noMatchTintColor);
                    }

                    value = colorFormatter.Format(color);
                    return true;
                case "isempty":
                    value = itemStack.IsEmpty() ? "true" : "false";
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        public override bool ParseAttribute(string name, string value, XUiController parent)
        {
            switch (name)
            {
                case "lockedslot_color":
                    lockedSlotColor = StringParsers.ParseColor32(value);
                    isLockedSlotColorSet = true;
                    return true;
                case "search_color":
                    searchColor = StringParsers.ParseColor32(value);
                    isSearchColorSet = true;
                    return true;
                case "nomatch_iconcolor":
                    noMatchTintColor = StringParsers.ParseColor32(value);
                    isNoMatchTintColorSet = true;
                    return true;
                case "select_color":
                    selectColor = StringParsers.ParseColor32(value);
                    return true;
                default:
                    return base.ParseAttribute(name, value, parent);
            }
        }

        public override void OnHovered(bool isOver)
        {
            this.isOver = isOver;
            base.OnHovered(isOver);
        }

        public override void SelectedChanged(bool _isSelected)
        {
            
        }

        protected new virtual void updateBorderColor()
        {
            if (IsDragAndDrop)
            {
                SelectionBorderColor = Color.clear;
            }
            else if (Selected)
            {
                SelectionBorderColor = selectColor;
            }
            else if (isOver)
            {
                SelectionBorderColor = highlightColor;
            }
            else if (IsHolding)
            {
                SelectionBorderColor = holdingColor;
            }
            else if (matchesSearch && isSearchColorSet)
            {
                SelectionBorderColor = searchColor;
            }
            else if (userLockedSlot && isLockedSlotColorSet)
            {
                SelectionBorderColor = lockedSlotColor;
            }
            else
            {
                SelectionBorderColor = backgroundColor;
            }
        }
    }
}

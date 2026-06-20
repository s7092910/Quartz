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

        private Color32 lockedSlotColor = new Color32(96, 96, 96, byte.MaxValue);
        private Color32 searchColor = new Color32(96, 96, 96, byte.MaxValue);
        private Color32 noMatchTintColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        protected bool isLockedSlotColorSet;
        protected bool isSearchColorSet;
        protected bool isNoMatchTintColorSet;

        private readonly CachedStringFormatterXuiRgbaColor colorFormatter = new CachedStringFormatterXuiRgbaColor();

        [XuiXmlBinding("issearchactive")]
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
                    RefreshBindings();
                }
            }
        }

        [XuiXmlBinding("matchessearch")]
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
                    RefreshBindings();
                }
            }
        }

        [XuiXmlAttribute("lockedslot_color")]
        public Color32 LockedSlotColor 
        { 
            get => lockedSlotColor;
            set
            {
                lockedSlotColor = value;
                isLockedSlotColorSet = true;
            }
        }

        [XuiXmlAttribute("search_color")]
        public Color32 SearchColor 
        { 
            get => searchColor;
            set
            {
                searchColor = value;
                isSearchColorSet = true;
            }
        }

        [XuiXmlAttribute("nomatch_iconcolor")]
        public Color32 NoMatchTintColor 
        { 
            get => noMatchTintColor; 
            set
            {
                noMatchTintColor = value;
                isNoMatchTintColorSet = true;
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

        public override void OnHovered(bool isOver)
        {
            this.isOver = isOver;
            base.OnHovered(isOver);
        }

        protected new virtual void updateBorderColor()
        {
            if (IsDragAndDrop)
            {
                SelectionBorderColor = Color.clear;
            }
            else if (IsSelected)
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

        [XuiXmlBinding("itemql")]
        private string GetItemQualityLevel()
        {
            if (itemClass == null || !ShowDurability)
            {
                return "";
            }

            return itemStack.itemValue.Quality > 0 ? itemcountFormatter.Format(itemStack.itemValue.Quality) : (itemStack.itemValue.IsMod ? "*" : "");
        }

        [XuiXmlBinding("stackcount")]
        private string GetStackCount()
        {
            if (itemClass == null || ShowDurability)
            {
                return "";
            }

            return itemClass.Stacknumber == 1 ? "" : itemcountFormatter.Format(itemStack.count);
        }

        [XuiXmlBinding("durabilitycolor")]
        private Color32 GetDurabilityColor()
        {
            Color32 color = QualityInfo.GetQualityColor(itemStack?.itemValue.Quality ?? 0);
            if (isSearchActive && !matchesSearch)
            {
                color = color.Over(noMatchTintColor);
            }

            return color;
        }

        [XuiXmlBinding("isempty")]
        private bool IsEmptySlot()
        {
            return itemStack.IsEmpty();
        }
    }
}

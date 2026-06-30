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

namespace Quartz
{
    public class XUiC_ItemStatEntry : XUiController
    {
        private Models.DisplayInfoEntry displayInfoEntry;

        private XUiC_ItemInfoWindow itemInfoWindow;

        private ItemStack itemStack;

        private string statValue;

        public DisplayInfoEntry DisplayInfoEntry
        {
            get
            {
                return displayInfoEntry;
            }
        }

        public XUiC_ItemInfoWindow ItemInfoWindow
        {
            get
            {
                return itemInfoWindow;
            }
            set
            {
                itemInfoWindow = value;
            }
        }

        [XuiXmlBinding("stat")]
        public string StatValue 
        { 
            get => statValue;
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (IsDirty)
            {
                statValue = GetStatValue();
                RefreshBindings();
                IsDirty = false;
            }
        }

        public void SetEntry(ItemStack itemStack, global::DisplayInfoEntry displayInfoEntry)
        {
            if (this.itemStack != itemStack)
            {
                this.itemStack = itemStack;
                IsDirty = true;
            }

            if (displayInfoEntry is Models.DisplayInfoEntry entry && this.displayInfoEntry != entry)
            {
                this.displayInfoEntry = entry;
                IsDirty = true;
            }
        }

        public void clearEntry()
        {
            itemStack = null;
            displayInfoEntry = null;
            IsDirty = true;
        }

        [XuiXmlBinding("isincrease")]
        private bool IsAnIncreaseFromCompare()
        {
            return statValue.Contains("[00FF00]");
        }

        [XuiXmlBinding("hasentry")]
        private bool HasStatEntry()
        {
            return itemStack != null && displayInfoEntry != null;
        }

        [XuiXmlBinding("title")]
        private string GetStatTitle()
        {
            if (displayInfoEntry == null)
            {
                return string.Empty;
            }
            if (displayInfoEntry.TitleOverride != null)
            {
                return displayInfoEntry.TitleOverride;
            }
            return UIDisplayInfoManager.Current.GetLocalizedName(displayInfoEntry.StatType);
        }

        private string GetStatValue()
        {
            if (itemStack == null || displayInfoEntry == null)
            {
                return string.Empty;
            }
            if (!itemInfoWindow.CompareStack.IsEmpty())
            {
                return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack.itemValue, itemInfoWindow.CompareStack.itemValue, xui.playerUI.entityPlayer, displayInfoEntry, false, false);
            }
            if (!itemInfoWindow.EquippedStack.IsEmpty())
            {
                return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack.itemValue, itemInfoWindow.EquippedStack.itemValue, xui.playerUI.entityPlayer, displayInfoEntry, true, false);
            }
            return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack.itemValue, itemInfoWindow.CompareStack.itemValue, xui.playerUI.entityPlayer, displayInfoEntry, false, true);
        }

        [XuiXmlBinding("icon")]
        private string GetStatIcon()
        {
            if (itemStack == null || displayInfoEntry == null)
            {
                return string.Empty;
            }

            return displayInfoEntry.icon;
        }

        [XuiXmlBinding("statmain")]
        private string GetStatValueMain()
        {
            if (itemStack == null || displayInfoEntry == null || itemInfoWindow.CompareStack.IsEmpty())
            {
                return string.Empty;
            }
            int sepIndex = statValue.IndexOf('(');
            if (sepIndex != -1)
            {
                return statValue.Substring(0, sepIndex - 1);
            }

            return statValue;

        }

        [XuiXmlBinding("statcompare")]
        private string GetStatValueCompare()
        {
            if (itemStack == null || displayInfoEntry == null || itemInfoWindow.CompareStack.IsEmpty())
            {
                return string.Empty;
            }

            int sepIndex = statValue.IndexOf('(');
            if (sepIndex != -1)
            {
                return statValue.Substring(sepIndex + 9, statValue.Length - sepIndex - 10);
            }

            return statValue;
        }
    }
}

/*Copyright 2026 Christopher Beda

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
    public class XUiC_AssembleItemStatEntry : XUiController
    {
        private Models.DisplayInfoEntry displayInfoEntry;

        private ItemStack itemStack;

        private string statValue;

        public DisplayInfoEntry DisplayInfoEntry
        {
            get
            {
                return displayInfoEntry;
            }
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

        public override bool GetBindingValueInternal(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "title":
                    value = GetStatTitle();
                    return true;
                case "icon":
                    value = GetStatIcon();
                    return true;
                case "stat":
                    value = statValue;
                    return true;
                case "hasentry":
                    value = HasStatEntry().ToString();
                    return true;
                default:
                    return base.GetBindingValueInternal(ref value, bindingName);
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

        private bool HasStatEntry()
        {
            return itemStack != null && displayInfoEntry != null;
        }

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

            return XUiM_ItemStack.GetStatItemValueTextWithModInfo(itemStack, xui.playerUI.entityPlayer, displayInfoEntry);

        }

        private string GetStatIcon()
        {
            if (itemStack == null || displayInfoEntry == null)
            {
                return string.Empty;
            }

            return displayInfoEntry.icon;
        }
    }
}

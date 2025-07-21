
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
    public class XUiC_ItemInfoWindow : global::XUiC_ItemInfoWindow
    {
        private ItemDisplayEntry displayEntry;

        private XUiC_ItemStatEntry[] itemStatControllers;

        private static ItemClass sledgeRobotItemClass;

        public override void Init()
        {
            base.Init();

            itemStatControllers = GetChildrenByType<XUiC_ItemStatEntry>();
            sledgeRobotItemClass = ItemClass.GetItemClass("gunBotT1JunkSledge");
            foreach(var controller in itemStatControllers )
            {
                controller.ItemInfoWindow = this;
            }
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            if (bindingName.StartsWith("itemstat") && bindingName.Contains("-"))
            {
                string[] split = bindingName.Split('-');
                int index = 0;
                if (split.Length == 2)
                {
                    if (int.TryParse(split[1], out index))
                    {
                        index--;
                    }
                }
                switch (split[0])
                {
                    case "itemstattitle":
                        value = XUiC_ItemInfoWindowPatch.GetStatTitle(this, index);
                        return true;
                    case "itemstaticon":
                        value = GetStatIcon(index);
                        return true;
                    case "itemstatmain":
                        value = GetStatValueMain(index);
                        return true;
                    case "itemstatcompare":
                        value = GetStatValueCompare(index);
                        return true;
                    case "itemstatincrease":
                        value = XUiC_ItemInfoWindowPatch.GetStatValue(this, index).Contains("[00FF00]").ToString();
                        return true;
                    case "itemstat":
                        value = XUiC_ItemInfoWindowPatch.GetStatValue(this, index);
                        return true;
                    default:
                        return base.GetBindingValue(ref value, bindingName);
                }
            }

            switch (bindingName)
            {
                case "itemammoname":
                    value = GetAmmoName();
                    return true;
                case "itemql":
                    value = "";
                    if (itemStack != null && !itemStack.IsEmpty() && itemClass != null && itemClass.ShowQualityBar)
                    {
                        value = itemStack.itemValue.Quality > 0 ? durabilitytextFormatter.Format(itemStack.itemValue.Quality) : itemStack.itemValue.IsMod ? "*" : "";
                    }
                    return true;
                case "stackcount":
                    value = "";
                    if (itemStack != null && !itemStack.IsEmpty() && itemClass != null && !itemClass.ShowQualityBar)
                    {
                        value = itemClass.Stacknumber == 1 ? "" : durabilitytextFormatter.Format(itemStack.count);
                    }
                    return true;
                case "weapontype":
                    value = "";
                    if (itemClass != null && itemClass.Properties.Contains("WeaponType"))
                    {
                        value = Localization.Get(itemClass.Properties.GetString("WeaponType"));
                    }
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        public void SetItemStats(ItemStack itemStack, ItemDisplayEntry itemDisplayEntry)
        {
            this.itemStack = itemStack;
            displayEntry = itemDisplayEntry;

            for (int i = 0; i < itemStatControllers.Length; i++)
            {
                if (displayEntry != null && i < displayEntry.DisplayStats.Count)
                {
                    itemStatControllers[i].SetEntry(this.itemStack, itemDisplayEntry.DisplayStats[i]);
                }
                else
                {
                    itemStatControllers[i].clearEntry();
                }
            }
        }

        public void RefreshItemStats()
        {
            foreach(XUiC_ItemStatEntry entry in itemStatControllers)
            {
                entry.IsDirty = true;
            }
        }

        private string GetStatIcon(int index)
        {
            if (displayEntry == null || displayEntry.DisplayStats.Count <= index)
            {
                return string.Empty;
            }
            Models.DisplayInfoEntry displayInfoEntry = displayEntry.DisplayStats[index] as Models.DisplayInfoEntry;
            if (displayInfoEntry != null && !string.IsNullOrEmpty(displayInfoEntry.icon))
            {
                return displayInfoEntry.icon;
            }
            return string.Empty;
        }

        private string GetStatValueMain(int index)
        {
            string value = XUiC_ItemInfoWindowPatch.GetStatValue(this, index);
            int sepIndex = value.IndexOf('(');
            if (sepIndex != -1)
            {
                value = value.Substring(0, sepIndex - 1);
            }

            return value;

        }

        private string GetStatValueCompare(int index)
        {
            if (CompareStack == ItemStack.Empty)
            {
                return string.Empty;
            }

            string value = XUiC_ItemInfoWindowPatch.GetStatValue(this, index);
            int sepIndex = value.IndexOf('(');
            if (sepIndex != -1)
            {
                value = value.Substring(sepIndex + 9, value.Length - sepIndex - 10);
            }

            return value;
        }

        private string GetAmmoName()
        {
            string value = string.Empty;
            if (itemClass != null && sledgeRobotItemClass.Id != itemClass.Id)
            {
                ItemActionRanged itemActionRanged = itemClass.Actions[0] as ItemActionRanged;
                if (itemActionRanged != null)
                {
                    if (itemActionRanged.MagazineItemNames.Length > 1)
                    {
                        ItemClass itemClass = ItemClass.GetItemClass(itemActionRanged.MagazineItemNames[itemStack.itemValue.SelectedAmmoTypeIndex], false);
                        value = itemClass.GetLocalizedItemName();
                    }
                }
                else
                {
                    ItemActionLauncher itemActionLauncher = itemClass.Actions[0] as ItemActionLauncher;
                    if (itemActionLauncher != null && itemActionLauncher.MagazineItemNames.Length > 1)
                    {
                        ItemClass itemClass2 = ItemClass.GetItemClass(itemActionLauncher.MagazineItemNames[itemStack.itemValue.SelectedAmmoTypeIndex], false);
                        value = itemClass2.GetLocalizedItemName();
                    }
                }
            }

            return value;
        }
    }
}
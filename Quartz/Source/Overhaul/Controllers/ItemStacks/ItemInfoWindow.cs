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

using QuartzOverhaul.InfoWindows;
using System;
using UnityEngine;

namespace QuartzOverhaul
{
    public class ItemInfoWindow : InfoWindow, IItemInfoWindowController
    {
        private ItemStack itemStack = ItemStack.Empty.Clone();

        private ItemClass itemClass;

        private IItemStackController selectedItemStack;

        private XUiController itemPreview;

        private XUiC_ItemActionList mainActionItemList;

        private XUiC_ItemActionList traderActionItemList;

        private XUiC_PartList partList;

        public XUiC_Counter BuySellCounter;

        private XUiController statButton;

        private XUiController descriptionButton;

        private InfoWindow emptyInfoWindow;

        private bool isBuying;

        private bool useCustomMarkup;

        public bool SetMaxCountOnDirty;

        private ItemDisplayEntry itemDisplayEntry;

        private SelectableEntry hoverEntry;

        private ItemStack compareStack = ItemStack.Empty;

        private bool showStats = true;

        private readonly CachedStringFormatter<int> itemcostFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

        private readonly CachedStringFormatter<int> markupFormatter = new CachedStringFormatter<int>((int _i) => (_i <= 0) ? ((_i >= 0) ? "" : $" ({_i}%)") : $" (+{_i}%)");

        private readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

        private readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

        private readonly CachedStringFormatter<float> durabilityfillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

        private readonly CachedStringFormatter<int> durabilitytextFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

        private readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

        private bool isOpenAsTrader
        {
            get
            {
                if (xui.Trader != null)
                {
                    return xui.Trader.Trader != null;
                }

                return false;
            }
        }

        public SelectableEntry HoverEntry
        {
            get
            {
                return hoverEntry;
            }
            set
            {
                if (hoverEntry == value)
                {
                    return;
                }

                hoverEntry = value;
                if (hoverEntry != null && !hoverEntry.IsSelected() && !itemStack.IsEmpty())
                {
                    global::ItemStack hoverControllerItemStack = GetHoverControllerItemStack();
                    if (!hoverControllerItemStack.IsEmpty() && XUiM_ItemStack.CanCompare(hoverControllerItemStack.itemValue.ItemClass, itemClass))
                    {
                        CompareStack = hoverControllerItemStack;
                    }
                    else
                    {
                        CompareStack = ItemStack.Empty;
                    }
                }
                else
                {
                    CompareStack = ItemStack.Empty;
                }
            }
        }

        public ItemStack CompareStack
        {
            get
            {
                return compareStack;
            }
            set
            {
                compareStack = value;
                RefreshBindings();
            }
        }

        public ItemStack EquippedStack
        {
            get
            {
                //if (compareStack.IsEmpty() && itemClass.EquipSlot != EnumEquipmentSlot.Count)
                //{
                //    EquipmentSlots slotFromStack = XUiM_PlayerEquipment.GetSlotFromStack(itemStack);
                //    return xui.PlayerEquipment.GetStackFromSlot(slotFromStack);
                //}

                return compareStack;
            }
        }

        private ItemStack GetHoverControllerItemStack()
        {
            return (hoverEntry as IItemStackController)?.ItemStack;
        }

        protected override void OnInit()
        {
            itemPreview = GetChildById("itemPreview");
            mainActionItemList = (XUiC_ItemActionList)GetChildById("itemActions");
            traderActionItemList = (XUiC_ItemActionList)GetChildById("vendorItemActions");
            partList = (XUiC_PartList)GetChildById("parts");
            BuySellCounter = GetChildByType<XUiC_Counter>();
            if (BuySellCounter != null)
            {
                BuySellCounter.OnCountChanged += Counter_OnCountChanged;
                BuySellCounter.Count = 1;
            }

            statButton = GetChildById("statButton");
            statButton.OnPress += StatButton_OnPress;
            descriptionButton = GetChildById("descriptionButton");
            descriptionButton.OnPress += DescriptionButton_OnPress;
        }

        private void DescriptionButton_OnPress(XUiController _sender, int _mouseButton)
        {
            ((XUiV_Button)statButton.ViewComponent).Selected = false;
            ((XUiV_Button)descriptionButton.ViewComponent).Selected = true;
            showStats = false;
            IsDirty = true;
        }

        private void StatButton_OnPress(XUiController _sender, int _mouseButton)
        {
            ((XUiV_Button)statButton.ViewComponent).Selected = true;
            ((XUiV_Button)descriptionButton.ViewComponent).Selected = false;
            showStats = true;
            IsDirty = true;
        }

        private void Counter_OnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
        {
            RefreshBindings();
            traderActionItemList.RefreshActionList();
        }

        public override void Deselect()
        {
        }

        protected override void OnUpdate(float dt)
        {
            if (IsDirty && ViewComponent.IsVisible)
            {
                if (emptyInfoWindow == null)
                {
                    emptyInfoWindow = (InfoWindow)xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
                }

                if (selectedItemStack != null)
                {
                    SetItemStack(selectedItemStack);
                }

                IsDirty = false;
            }
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "itemname":
                    value = ((this.itemClass != null) ? this.itemClass.GetLocalizedItemName() : "");
                    return true;
                case "itemammoname":
                    value = "";
                    if (this.itemClass != null)
                    {
                        ItemActionRanged itemActionRanged = this.itemClass.Actions[0] as ItemActionRanged;
                        if (itemActionRanged != null)
                        {
                            if (itemActionRanged.MagazineItemNames.Length > 1)
                            {
                                ItemClass itemClass = ItemClass.GetItemClass(itemActionRanged.MagazineItemNames[itemStack.itemValue.SelectedAmmoTypeIndex]);
                                value = itemClass.GetLocalizedItemName();
                            }
                        }
                        else
                        {
                            ItemActionLauncher itemActionLauncher = this.itemClass.Actions[0] as ItemActionLauncher;
                            if (itemActionLauncher != null && itemActionLauncher.MagazineItemNames.Length > 1)
                            {
                                ItemClass itemClass2 = ItemClass.GetItemClass(itemActionLauncher.MagazineItemNames[itemStack.itemValue.SelectedAmmoTypeIndex]);
                                value = itemClass2.GetLocalizedItemName();
                            }
                        }
                    }

                    return true;
                case "itemicon":
                    if (itemStack != null)
                    {
                        value = itemStack.itemValue.GetPropertyOverride("CustomIcon", (itemStack.itemValue.ItemClass != null) ? itemStack.itemValue.ItemClass.GetIconName() : "");
                    }
                    else
                    {
                        value = "";
                    }

                    return true;
                case "itemcost":
                    value = "";
                    if (this.itemClass != null)
                    {
                        if (!((!this.itemClass.IsBlock()) ? this.itemClass.SellableToTrader : Block.list[itemStack.itemValue.type].SellableToTrader))
                        {
                            value = Localization.Get("xuiNoSellPrice");
                            return true;
                        }

                        int count2 = itemStack.count;
                        if (isOpenAsTrader)
                        {
                            count2 = BuySellCounter.Count;
                        }

                        if (isBuying)
                        {
                            if (useCustomMarkup)
                            {
                                value = itemcostFormatter.Format(XUiM_Trader.GetBuyPrice(base.xui, itemStack.itemValue, count2, this.itemClass, selectedItemStack.SlotIndex));
                                return true;
                            }

                            value = itemcostFormatter.Format(XUiM_Trader.GetBuyPrice(base.xui, itemStack.itemValue, count2, this.itemClass));
                        }
                        else
                        {
                            int sellPrice = XUiM_Trader.GetSellPrice(base.xui, itemStack.itemValue, count2, this.itemClass);
                            value = ((sellPrice > 0) ? itemcostFormatter.Format(sellPrice) : Localization.Get("xuiNoSellPrice"));
                        }
                    }

                    return true;
                case "pricelabel":
                    value = "";
                    if (this.itemClass != null)
                    {
                        if (!((!this.itemClass.IsBlock()) ? this.itemClass.SellableToTrader : Block.list[itemStack.itemValue.type].SellableToTrader))
                        {
                            return true;
                        }

                        int count = itemStack.count;
                        if (isOpenAsTrader)
                        {
                            count = BuySellCounter.Count;
                        }

                        if (isBuying)
                        {
                            value = ((XUiM_Trader.GetBuyPrice(base.xui, itemStack.itemValue, count, this.itemClass) > 0) ? Localization.Get("xuiBuyPrice") : "");
                        }
                        else
                        {
                            value = ((XUiM_Trader.GetSellPrice(base.xui, itemStack.itemValue, count, this.itemClass) > 0) ? Localization.Get("xuiSellPrice") : "");
                        }
                    }

                    return true;
                case "markup":
                    value = "";
                    if (useCustomMarkup)
                    {
                        int v = xui.Trader.Trader.GetMarkupByIndex(selectedItemStack.SlotIndex) * 20;
                        value = markupFormatter.Format(v);
                    }

                    return true;
                case "itemicontint":
                    {
                        Color32 v3 = Color.white;
                        if (this.itemClass != null)
                        {
                            v3 = itemStack.itemValue.ItemClass.GetIconTint(itemStack.itemValue);
                        }

                        value = itemicontintcolorFormatter.Format(v3);
                        return true;
                    }
                case "itemdescription":
                    value = "";
                    if (this.itemClass != null)
                    {
                        if (this.itemClass.IsBlock())
                        {
                            string descriptionKey = Block.list[this.itemClass.Id].DescriptionKey;
                            if (Localization.Exists(descriptionKey))
                            {
                                value = Localization.Get(descriptionKey);
                            }
                        }
                        else
                        {
                            string descriptionKey2 = this.itemClass.DescriptionKey;
                            if (Localization.Exists(descriptionKey2))
                            {
                                value = Localization.Get(descriptionKey2);
                            }
                        }
                    }

                    return true;
                case "itemgroupicon":
                    value = "";
                    if (this.itemClass != null && this.itemClass.Groups.Length != 0)
                    {
                        switch (this.itemClass.Groups[0].ToLower())
                        {
                            case "basics":
                                value = "ui_game_symbol_campfire";
                                break;
                            case "building":
                                value = "ui_game_symbol_map_house";
                                break;
                            case "resources":
                                value = "ui_game_symbol_resource";
                                break;
                            case "ammo/weapons":
                                value = "ui_game_symbol_knife";
                                break;
                            case "tools/traps":
                                value = "ui_game_symbol_tool";
                                break;
                            case "food/cooking":
                                value = "ui_game_symbol_fork";
                                break;
                            case "medicine":
                                value = "ui_game_symbol_medical";
                                break;
                            case "clothing":
                                value = "ui_game_symbol_shirt";
                                break;
                            case "decor/miscellaneous":
                                value = "ui_game_symbol_chair";
                                break;
                            case "books":
                                value = "ui_game_symbol_book";
                                break;
                            case "chemicals":
                                value = "ui_game_symbol_water";
                                break;
                            case "mods":
                                value = "ui_game_symbol_assemble";
                                break;
                            default:
                                value = "ui_game_symbol_campfire";
                                break;
                        }
                    }

                    return true;
                case "hasdurability":
                    value = (!itemStack.IsEmpty() && this.itemClass.ShowQualityBar).ToString();
                    return true;
                case "durabilitycolor":
                    {
                        Color32 v2 = Color.white;
                        if (!itemStack.IsEmpty())
                        {
                            v2 = QualityInfo.GetTierColor(itemStack.itemValue.Quality);
                        }

                        value = durabilitycolorFormatter.Format(v2);
                        return true;
                    }
                case "durabilityfill":
                    value = (itemStack.IsEmpty() ? "" : ((itemStack.itemValue.MaxUseTimes == 0) ? "1" : durabilityfillFormatter.Format(((float)itemStack.itemValue.MaxUseTimes - itemStack.itemValue.UseTimes) / (float)itemStack.itemValue.MaxUseTimes)));
                    return true;
                case "durabilityjustify":
                    value = "center";
                    if (!itemStack.IsEmpty() && !this.itemClass.ShowQualityBar)
                    {
                        value = "right";
                    }

                    return true;
                case "durabilitytext":
                    value = "";
                    if (!itemStack.IsEmpty())
                    {
                        if (this.itemClass.ShowQualityBar)
                        {
                            value = ((itemStack.itemValue.Quality > 0) ? durabilitytextFormatter.Format(itemStack.itemValue.Quality) : "-");
                        }
                        else
                        {
                            value = ((this.itemClass.Stacknumber == 1) ? "" : durabilitytextFormatter.Format(itemStack.count));
                        }
                    }

                    return true;
                case "itemtypeicon":
                    if (itemStack.IsEmpty())
                    {
                        value = "";
                    }
                    else if (itemStack.itemValue.ItemClass.IsBlock())
                    {
                        value = Block.list[itemStack.itemValue.type].ItemTypeIcon;
                    }
                    else
                    {
                        if (itemStack.itemValue.ItemClass.AltItemTypeIcon != null && itemStack.itemValue.ItemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, itemStack.itemValue.ItemClass, itemStack.itemValue))
                        {
                            value = itemStack.itemValue.ItemClass.AltItemTypeIcon;
                            return true;
                        }

                        value = itemStack.itemValue.ItemClass.ItemTypeIcon;
                    }

                    return true;
                case "hasitemtypeicon":
                    if (itemStack.IsEmpty())
                    {
                        value = "false";
                    }
                    else if (itemStack.itemValue.ItemClass.IsBlock())
                    {
                        value = (Block.list[itemStack.itemValue.type].ItemTypeIcon != "").ToString();
                    }
                    else
                    {
                        value = (itemStack.itemValue.ItemClass.ItemTypeIcon != "").ToString();
                    }

                    return true;
                case "itemtypeicontint":
                    value = "255,255,255,255";
                    if (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, itemStack.itemValue.ItemClass, itemStack.itemValue))
                    {
                        value = altitemtypeiconcolorFormatter.Format(itemStack.itemValue.ItemClass.AltItemTypeIconColor);
                    }

                    return true;
                case "shownormaloptions":
                    value = (!isOpenAsTrader).ToString();
                    return true;
                case "showtraderoptions":
                    value = isOpenAsTrader.ToString();
                    return true;
                case "showstats":
                    value = showStats.ToString();
                    return true;
                case "showdescription":
                    value = (!showStats).ToString();
                    return true;
                case "iscomparing":
                    value = (!CompareStack.IsEmpty()).ToString();
                    return true;
                case "isnotcomparing":
                    value = CompareStack.IsEmpty().ToString();
                    return true;
                case "showstatoptions":
                    value = "false";
                    return true;
                case "showonlydescription":
                    value = (!XUiM_ItemStack.HasItemStats(itemStack)).ToString();
                    return true;
                case "showstatanddescription":
                    value = XUiM_ItemStack.HasItemStats(itemStack).ToString();
                    return true;
                case "itemstattitle1":
                    value = ((this.itemClass != null) ? GetStatTitle(0) : "");
                    return true;
                case "itemstat1":
                    value = ((this.itemClass != null) ? GetStatValue(0) : "");
                    return true;
                case "itemstattitle2":
                    value = ((this.itemClass != null) ? GetStatTitle(1) : "");
                    return true;
                case "itemstat2":
                    value = ((this.itemClass != null) ? GetStatValue(1) : "");
                    return true;
                case "itemstattitle3":
                    value = ((this.itemClass != null) ? GetStatTitle(2) : "");
                    return true;
                case "itemstat3":
                    value = ((this.itemClass != null) ? GetStatValue(2) : "");
                    return true;
                case "itemstattitle4":
                    value = ((this.itemClass != null) ? GetStatTitle(3) : "");
                    return true;
                case "itemstat4":
                    value = ((this.itemClass != null) ? GetStatValue(3) : "");
                    return true;
                case "itemstattitle5":
                    value = ((this.itemClass != null) ? GetStatTitle(4) : "");
                    return true;
                case "itemstat5":
                    value = ((this.itemClass != null) ? GetStatValue(4) : "");
                    return true;
                case "itemstattitle6":
                    value = ((this.itemClass != null) ? GetStatTitle(5) : "");
                    return true;
                case "itemstat6":
                    value = ((this.itemClass != null) ? GetStatValue(5) : "");
                    return true;
                case "itemstattitle7":
                    value = ((this.itemClass != null) ? GetStatTitle(6) : "");
                    return true;
                case "itemstat7":
                    value = ((this.itemClass != null) ? GetStatValue(6) : "");
                    return true;
                default:
                    return false;
            }
        }

        private string GetStatTitle(int index)
        {
            if (itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
            {
                return "";
            }

            if (itemDisplayEntry.DisplayStats[index].TitleOverride != null)
            {
                return itemDisplayEntry.DisplayStats[index].TitleOverride;
            }

            return UIDisplayInfoManager.Current.GetLocalizedName(itemDisplayEntry.DisplayStats[index].StatType);
        }

        private string GetStatValue(int index)
        {
            if (itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
            {
                return "";
            }

            DisplayInfoEntry infoEntry = itemDisplayEntry.DisplayStats[index];
            if (!CompareStack.IsEmpty())
            {
                return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack.itemValue, CompareStack.itemValue, base.xui.playerUI.entityPlayer, infoEntry, flipCompare: false, useMods: false);
            }

            if (!EquippedStack.IsEmpty())
            {
                return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack.itemValue, EquippedStack.itemValue, base.xui.playerUI.entityPlayer, infoEntry, flipCompare: true, useMods: false);
            }

            return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack.itemValue, CompareStack.itemValue, base.xui.playerUI.entityPlayer, infoEntry);
        }

        private void makeVisible(bool _makeVisible)
        {
            if (_makeVisible && windowGroup.isShowing)
            {
                base.ViewComponent.IsVisible = true;
                ((XUiV_Window)viewComponent).ForceVisible(1f);
            }
        }


        public void SetItemStack(IItemStackController stack, bool _makeVisible = false)
        {
            if (stack == null || stack.ItemStack.IsEmpty())
            {
                ShowEmptyInfo();
                return;
            }
            makeVisible(_makeVisible);

            //SetInfo(stack.ItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.Item);
        }

        public void SetItemStack(XUiC_EquipmentStack stack, bool _makeVisible = false)
        {
            SetInfo(stack.ItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.Equipment);
        }

        public void SetItemStack(XUiC_BasePartStack stack, bool _makeVisible = false)
        {
            SetInfo(stack.ItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.Part);
        }

        public void SetItemStack(XUiC_TraderItemEntry stack, bool _makeVisible = false)
        {
            SetInfo(stack.Item, stack, XUiC_ItemActionList.ItemActionListTypes.Trader);
        }

        public void SetItemStack(XUiC_QuestTurnInEntry stack, bool _makeVisible = false)
        {
            SetInfo(stack.Item, stack, XUiC_ItemActionList.ItemActionListTypes.QuestReward);
        }

        private void ShowEmptyInfo()
        {
            if (emptyInfoWindow == null)
            {
                emptyInfoWindow = (InfoWindow)xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
            }

            emptyInfoWindow.ViewComponent.IsVisible = true;
        }

        private void SetInfo(ItemStack stack, XUiController controller, XUiC_ItemActionList.ItemActionListTypes actionListType)
        {
            bool flag = stack.itemValue.type == itemStack.itemValue.type && stack.count == itemStack.count;
            itemStack = stack.Clone();
            bool flag2 = itemStack != null && !itemStack.IsEmpty();
            if (itemPreview == null)
            {
                return;
            }

            if (!flag || !stack.itemValue.Equals(itemStack.itemValue))
            {
                compareStack = ItemStack.Empty.Clone();
            }

            itemClass = null;
            int num = 1;
            if (flag2)
            {
                itemClass = itemStack.itemValue.ItemClass;
                if (itemClass is ItemClassQuest)
                {
                    itemClass = ItemClassQuest.GetItemQuestById(itemStack.itemValue.Seed);
                }

                num = (itemClass.IsBlock() ? Block.list[itemStack.itemValue.type].EconomicBundleSize : itemClass.EconomicBundleSize);
            }

            if (flag2)
            {
                itemDisplayEntry = UIDisplayInfoManager.Current.GetDisplayStatsForTag(itemClass.IsBlock() ? Block.list[itemStack.itemValue.type].DisplayType : itemClass.DisplayType);
            }

            if (isOpenAsTrader)
            {
                isBuying = actionListType == XUiC_ItemActionList.ItemActionListTypes.Trader;

                useCustomMarkup = isBuying 
                    && xui.Trader.TraderTileEntity is TileEntityVendingMachine 
                    && (xui.Trader.Trader.TraderInfo.PlayerOwned 
                    || xui.Trader.Trader.TraderInfo.Rentable);

                traderActionItemList.SetCraftingActionList(actionListType, controller);
                int count = BuySellCounter.Count;
                if (!flag)
                {
                    BuySellCounter.Count = ((itemStack.count >= num) ? num : 0);
                }
                else if (count > itemStack.count)
                {
                    BuySellCounter.Count = ((itemStack.count >= num) ? itemStack.count : 0);
                }

                BuySellCounter.MaxCount = ((itemStack.count >= num) ? (itemStack.count / num * num) : 0);
                BuySellCounter.Step = num;
                if (BuySellCounter.Count == 0 && itemStack.count >= num)
                {
                    BuySellCounter.Count = num;
                }

                if (SetMaxCountOnDirty)
                {
                    BuySellCounter.Count = BuySellCounter.MaxCount;
                    SetMaxCountOnDirty = false;
                }
            }
            else
            {
                mainActionItemList.SetCraftingActionList(actionListType, controller);
                isBuying = false;
                useCustomMarkup = false;
            }

            if (flag2 && itemStack.itemValue.Modifications != null)
            {
                //partList.SetMainItem(itemStack);
                if (itemStack.itemValue.CosmeticMods != null && itemStack.itemValue.CosmeticMods.Length != 0 && itemStack.itemValue.CosmeticMods[0] != null && !itemStack.itemValue.CosmeticMods[0].IsEmpty())
                {
                    partList.SetSlot(itemStack.itemValue.CosmeticMods[0], 0);
                    partList.SetSlots(itemStack.itemValue.Modifications, 1);
                }
                else
                {
                    partList.SetSlots(itemStack.itemValue.Modifications);
                }

                partList.ViewComponent.IsVisible = true;
            }
            else
            {
                partList.ViewComponent.IsVisible = false;
            }

            RefreshBindings();
        }

        public void SetItemStack(IItemStackController stack, XUiController controller, XUiC_ItemActionList.ItemActionListTypes actionListType, bool _makeVisible = false)
        {
            throw new NotImplementedException();
        }
    }

    public interface IItemInfoWindowController
    {
        void SetItemStack(IItemStackController stack, XUiController controller, XUiC_ItemActionList.ItemActionListTypes actionListType, bool _makeVisible = false);
    }
}

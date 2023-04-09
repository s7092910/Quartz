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

using System;
using UnityEngine;

namespace Quartz
{
    public class XUiC_HUDActiveItem : XUiController
    {
        private string statAtlas = "ItemIconAtlas";

        private string lastAmmoName = "";
        private int currentAmmoCount;
        private ItemValue itemValue;
        private ItemClass displayItemClass;
        private ItemAction itemAction;
        private ItemClass heldItemClass;
        private float oldValue;
        private int currentSlotIndex = -1;

        private string entityDamage;
        private string blockDamage;

        private static PassiveEffects peBlockDamage = (PassiveEffects)Enum.Parse(typeof(PassiveEffects), "BlockDamage");
        private static PassiveEffects peEntityDamage = (PassiveEffects)Enum.Parse(typeof(PassiveEffects), "EntityDamage");

        private EntityPlayer localPlayer;

        private readonly CachedStringFormatter<int> statcurrentFormatterInt = new CachedStringFormatter<int>((int _i) => _i.ToString());
        private readonly CachedStringFormatter<int> currentPaintAmmoFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());
        private readonly CachedStringFormatter<int, int> statcurrentWMaxFormatterAOfB = new CachedStringFormatter<int, int>((int _i, int _i1) => $"{_i}/{_i1}");
        private readonly CachedStringFormatterXuiRgbaColor staticoncolorFormatter = new CachedStringFormatterXuiRgbaColor();
        private readonly CachedStringFormatter<int> levelFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString("+0;-#"));

        public override void Init()
        {
            base.Init();
            IsDirty = true;
            itemValue = ItemValue.None.Clone();
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (localPlayer == null && XUi.IsGameRunning())
            {
                localPlayer = xui.playerUI.entityPlayer;
                IsDirty = true;
            }

            if (currentSlotIndex != xui.PlayerInventory.Toolbelt.GetFocusedItemIdx())
            {
                currentSlotIndex = xui.PlayerInventory.Toolbelt.GetFocusedItemIdx();
                IsDirty = true;
            }

            if (IsDirty || HasChanged())
            {
                SetupActiveItemEntry();
                updateActiveItemAmmo();
                RefreshBindings(true);
                IsDirty = false;
            }
        }
        public override void OnOpen()
        {
            base.OnOpen();
            xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
            xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
            IsDirty = true;
        }

        public override void OnClose()
        {
            base.OnClose();
            xui.PlayerInventory.OnBackpackItemsChanged -= PlayerInventory_OnBackpackItemsChanged;
            xui.PlayerInventory.OnToolbeltItemsChanged -= PlayerInventory_OnToolbeltItemsChanged;
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "loadedammo":
                    value = GetLoadedAmmo();
                    return true;
                case "totalammo":
                    value = GetTotalAmmo();
                    return true;
                case "staticon":
                    value = (displayItemClass != null) ? displayItemClass.GetIconName() : "";
                    return true;
                case "staticonatlas":
                    value = statAtlas;
                    return true;
                case "staticoncolor":
                    Color32 v2 = ((displayItemClass != null) ? displayItemClass.GetIconTint() : Color.white);
                    value = staticoncolorFormatter.Format(v2);
                    return true;
                case "statvisible":
                    value = IsStatVisible().ToString();
                    return true;
                case "isgun":
                    value = (heldItemClass != null && heldItemClass.IsGun()).ToString();
                    return true;
                case "istool":
                    value = IsToolHeld().ToString();
                    return true;
                case "ismelee":
                    value = IsMeleeHeld().ToString();
                    return true;
                case "entitydamage":
                    value = entityDamage;
                    return true;
                case "blockdamage":
                    value = blockDamage;
                    return true;
                case "elevation":
                    value = "";
                    if (XUi.IsGameRunning() && localPlayer != null)
                    {
                        int v = Mathf.RoundToInt(localPlayer.GetPosition().y - WeatherManager.SeaLevel());
                        value = levelFormatter.Format(v);
                    }
                    return true;
                default:
                    return false;
            }
        }

        private bool IsStatVisible()
        {
            if (localPlayer == null)
            {
                return true;
            }

            if (localPlayer.IsDead())
            {
                return false;
            }

            return heldItemClass != null;
        }

        private string GetLoadedAmmo()
        {
            string currentStat = "";
            if (localPlayer == null)
            {
                return currentStat;
            }

            if (itemAction is ItemActionTextureBlock)
            {
                currentStat = currentPaintAmmoFormatter.Format(currentAmmoCount);
            }
            else
            {
                currentStat = statcurrentFormatterInt.Format(localPlayer.inventory.holdingItemItemValue.Meta);
            }

            return currentStat;
        }

        private string GetTotalAmmo()
        {
            string maxStat = "";
            if (localPlayer == null)
            {
                return maxStat;
            }

            maxStat = statcurrentFormatterInt.Format(currentAmmoCount);

            return maxStat;
        }

        private bool IsToolHeld()
        {
            return heldItemClass != null && heldItemClass.HasAnyTags(FastTags.Parse("tool")) && !heldItemClass.IsGun();
        }

        private bool IsMeleeHeld()
        {
            return heldItemClass != null && heldItemClass.IsDynamicMelee() && !IsToolHeld();
        }

        private void SetupActiveItemEntry()
        {
            heldItemClass = null;
            displayItemClass = null;
            itemAction = null;

            entityDamage = string.Empty;
            blockDamage = string.Empty;

            if ((localPlayer == null) || localPlayer.inventory.GetItemInSlot(currentSlotIndex) == null)
            {
                itemValue = ItemValue.None.Clone();
                return;
            }

            itemValue = localPlayer.inventory.GetItem(currentSlotIndex).itemValue;
            if (itemValue.ItemClass != null)
            {
                heldItemClass = itemValue.ItemClass;
                if (itemValue.ItemClass.IsGun())
                {
                    ItemActionAttack itemActionAttack = itemValue.ItemClass.Actions[0] as ItemActionAttack;
                    if (itemActionAttack == null || itemActionAttack is ItemActionMelee || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, localPlayer.inventory.holdingItemItemValue, 0f, localPlayer) <= 0)
                    {
                        currentAmmoCount = 0;
                        return;
                    }

                    if (itemActionAttack.MagazineItemNames != null && itemActionAttack.MagazineItemNames.Length != 0)
                    {
                        lastAmmoName = itemActionAttack.MagazineItemNames[itemValue.SelectedAmmoTypeIndex];
                        itemValue = ItemClass.GetItem(lastAmmoName);
                        displayItemClass = ItemClass.GetItemClass(lastAmmoName);
                    }

                    itemAction = itemActionAttack;
                }
                else if (itemValue.ItemClass.IsDynamicMelee() || itemValue.ItemClass.HasAnyTags(FastTags.Parse("tool")))
                {
                    if (itemValue.ItemClass.GetIconName() == "missingIcon")
                    {
                        return;
                    }

                    itemAction = itemValue.ItemClass.Actions[0];
                    displayItemClass = itemValue.ItemClass;

                    entityDamage = GetEntityDamage();
                    blockDamage = GetBlockDamage();
                }
            }
            else
            {
                currentAmmoCount = 0;
            }
        }

        private void updateActiveItemAmmo()
        {
            if (heldItemClass != null && heldItemClass.IsGun() && itemValue.type != 0)
            {
                currentAmmoCount = localPlayer.inventory.GetItemCount(itemValue);
                currentAmmoCount += localPlayer.bag.GetItemCount(itemValue);
                IsDirty = true;
            }
        }
        private string GetEntityDamage()
        {
            if (localPlayer != null && itemValue != null)
            {
                return EffectManager.GetValue(peEntityDamage, itemValue, 0f, localPlayer).ToString("0.#");
            }

            return string.Empty;
        }

        private string GetBlockDamage()
        {
            if (localPlayer != null && itemValue != null)
            {
                return EffectManager.GetValue(peBlockDamage, itemValue, 0f, localPlayer).ToString("0.#");
            }

            return string.Empty;
        }

        private bool HasChanged()
        {
            bool result = false;
            if (localPlayer.inventory.holdingItemItemValue.ItemClass.Actions[0] is ItemActionRanged)
            {
                result = oldValue != localPlayer.inventory.holdingItemItemValue.Meta;
                oldValue = localPlayer.inventory.holdingItemItemValue.Meta;
            }
            else if (IsToolHeld())
            {
                float elevation = Mathf.RoundToInt(localPlayer.GetPosition().y - WeatherManager.SeaLevel());
                result = oldValue != elevation;
                oldValue = elevation;
            }
            else if (IsMeleeHeld())
            {
                result = entityDamage != GetEntityDamage() || blockDamage != GetBlockDamage();
            }

            return result;
        }

        private void PlayerInventory_OnToolbeltItemsChanged()
        {
            IsDirty = true;
        }

        private void PlayerInventory_OnBackpackItemsChanged()
        {
            IsDirty = true;
        }
    }
}

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

        [XuiXmlBinding("entitydamage")]
        public string EntityDamage { get => entityDamage; }

        [XuiXmlBinding("blockdamage")]
        public string BlockDamage { get => blockDamage; }

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
                RefreshBindings();
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

        [XuiXmlBinding("staticon")]
        private string GetStatIcon()
        {
            return displayItemClass != null ? displayItemClass.GetIconName() : "";
        }

        [XuiXmlBinding("staticonatlas")]
        private string GetStatIconAtlas()
        {
            return statAtlas;
        }

        [XuiXmlBinding("staticoncolor")]
        private Color32 GetStatIconColor()
        {
            return displayItemClass != null ? displayItemClass.GetIconTint() : Color.white;
        }

        [XuiXmlBinding("statvisible")]
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

        [XuiXmlBinding("loadedAmmo")]
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

        [XuiXmlBinding("totalammo")]
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

        [XuiXmlBinding("isgun")]
        private bool IsGunHeld()
        {
            return heldItemClass != null && heldItemClass.IsGun();
        }

        [XuiXmlBinding("istool")]
        private bool IsToolHeld()
        {
            return heldItemClass != null && heldItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("tool")) && !heldItemClass.IsGun();
        }

        [XuiXmlBinding("ismelee")]
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
                itemValue = ItemValue.None;
                return;
            }

            itemValue = localPlayer.inventory.GetItem(currentSlotIndex).itemValue;
            if (itemValue.ItemClass != null)
            {
                heldItemClass = itemValue.ItemClass;
                if (itemValue.ItemClass.IsGun())
                {
                    ItemActionAttack itemActionAttack = itemValue.ItemClass.Actions[0] as ItemActionAttack;
                    if (itemActionAttack == null || itemActionAttack is ItemActionMelee || itemActionAttack.InfiniteAmmo && !itemActionAttack.ForceShowAmmo || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, localPlayer.inventory.holdingItemItemValue, 0f, localPlayer) <= 0)
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
                else if (itemValue.ItemClass.IsDynamicMelee() || itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("tool")))
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

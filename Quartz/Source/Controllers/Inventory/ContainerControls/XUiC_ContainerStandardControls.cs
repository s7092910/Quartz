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

using HarmonyLib;
using Quartz.Inventory;
using System;
using System.Reflection;
using UnityEngine;

namespace Quartz
{
    public class XUiC_ContainerStandardControls : global::XUiC_ContainerStandardControls
    {
        protected int ignoredLockedSlots;
        protected static float lastStashTime;

        protected XUiV_Button btnIndividualLock;
        protected XUiC_ComboBoxInt comboBox;

        protected ILockableInventory inventory;

        public override void Init()
        {
            base.Init();

            XUiController child = GetChildById("btnMoveAll");
            if (child != null)
            {
                ClearEventHandlers(child, "OnPress");
                child.OnPress += MoveAll;
            }

            child = GetChildById("btnMoveFillAndSmart");
            if (child != null)
            {
                ClearEventHandlers(child, "OnPress");
                child.OnPress += MoveFillAndSmart;
            }

            child = GetChildById("btnMoveFillStacks");
            if (child != null)
            {
                ClearEventHandlers(child, "OnPress");
                child.OnPress += MoveFillStacks;
            }

            child = GetChildById("btnMoveSmart");
            if (child != null)
            {
                ClearEventHandlers(child, "OnPress");
                child.OnPress += MoveSmart;
            }

            child = GetChildById("cbxLockedSlots");
            if (child != null)
            {
                comboBox = GetChildByType<XUiC_ComboBoxInt>();
                if(comboBox != null)
                {
                    comboBox.OnValueChanged += ComboBoxOnValueChanged;
                }
            }

            child = GetChildById("btnIndividualLock");
            if(child != null)
            {
                child.OnPress += IndividualLockOnPress;
                btnIndividualLock = child.ViewComponent as XUiV_Button;
            }

            inventory = GetParentWindow().Controller.GetChildByType<XUiC_ItemStackGrid>() as ILockableInventory;
        }

        public override void OnOpen()
        {
            base.OnOpen();
            RefreshBindings();
        }

        public void MoveAllButLocked()
        {
            MoveAll(this, 0);
        }

        public bool IsIndividualSlotLockingAllowed()
        {
            return btnIndividualLock != null && btnIndividualLock.Selected;
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch(bindingName)
            {
                case "totallockedslotscount":
                    value = inventory != null ? inventory.TotalLockedSlotsCount().ToString() : "0";
                    return true;
                case "combolockedslots":
                    value = ignoredLockedSlots.ToString();
                    return true;
                case "individuallockedslotscount":
                    value = inventory != null ? inventory.IndividualLockedSlotsCount().ToString() : "0";
                    return true;
                case "unlockedslotscount":
                    value = inventory != null ? inventory.UnlockedSlotCount().ToString() : "0";
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        public new void ChangeLockedSlots(long newValue)
        {
            ignoredLockedSlots = (int)newValue;
            RefreshBindings();
        }

        public void ChangeLockingStatus(bool enableLocking)
        {
            if(comboBox != null)
            {
                comboBox.Enabled = enableLocking;
            }

            if(btnIndividualLock != null)
            {
                btnIndividualLock.Enabled = enableLocking;
            }
        }

        protected virtual void IndividualLockOnPress(XUiController _sender, int _mouseButton)
        {
            if(btnIndividualLock != null)
            {
                btnIndividualLock.Selected = !btnIndividualLock.Selected;
            }
        }

        protected virtual void ComboBoxOnValueChanged(XUiController sender, long oldValue, long newValue)
        {
            ChangeLockedSlots(newValue);
        }

        protected virtual void MoveSmart(XUiController sender, int mouseButton)
        {
            XUiC_ItemStackGrid srcGrid;
            IInventory dstInventory;
            if (MoveAllowed(out _, out srcGrid, out dstInventory))
            {
                StashItems(srcGrid, dstInventory, ignoredLockedSlots, XUiM_LootContainer.EItemMoveKind.FillAndCreate, MoveStartBottomRight);
            }
        }

        protected virtual void MoveFillStacks(XUiController sender, int mouseButton)
        {
            XUiC_ItemStackGrid srcGrid;
            IInventory dstInventory;
            if (MoveAllowed(out _, out srcGrid, out dstInventory))
            {
                StashItems(srcGrid, dstInventory, ignoredLockedSlots, XUiM_LootContainer.EItemMoveKind.FillOnly, MoveStartBottomRight);
            }
        }

        protected virtual void MoveFillAndSmart(XUiController sender, int mouseButton)
        {
            XUiC_ItemStackGrid srcGrid;
            IInventory dstInventory;

            float unscaledTime = Time.unscaledTime;
            XUiM_LootContainer.EItemMoveKind moveKind = XUiM_LootContainer.EItemMoveKind.FillOnlyFirstCreateSecond;
            if (unscaledTime - lastStashTime < 2f)
            {
                moveKind = XUiM_LootContainer.EItemMoveKind.FillAndCreate;
            }

            if (MoveAllowed(out _, out srcGrid, out dstInventory))
            {
                StashItems(srcGrid, dstInventory, ignoredLockedSlots, moveKind, MoveStartBottomRight);
                lastStashTime = unscaledTime;
            }
        }

        protected virtual void MoveAll(XUiController sender, int mouseButton)
        {
            XUiC_ItemStackGrid srcGrid;
            IInventory dstInventory;
            if (MoveAllowed(out _, out srcGrid, out dstInventory))
            {
                ValueTuple<bool, bool> valueTuple = StashItems(srcGrid, dstInventory, ignoredLockedSlots, XUiM_LootContainer.EItemMoveKind.All, MoveStartBottomRight);
                bool item = valueTuple.Item1;
                bool item2 = valueTuple.Item2;
                Action<bool, bool> moveAllDone = MoveAllDone;
                if (moveAllDone == null)
                {
                    return;
                }
                moveAllDone(item, item2);
            }
        }

        private (bool allMoved, bool anyMoved) StashItems(XUiC_ItemStackGrid srcGrid, IInventory dstInventory, int ignoredSlots, XUiM_LootContainer.EItemMoveKind moveKind, bool startBottomRight)
        {
            if (srcGrid == null || dstInventory == null)
            {
                return (false, false);
            }
            XUiController[] itemStackControllers = srcGrid.GetItemStackControllers();

            bool item = true;
            bool item2 = false;
            int num = startBottomRight ? (itemStackControllers.Length - 1) : ignoredSlots;
            while (startBottomRight ? (num >= ignoredSlots) : (num < itemStackControllers.Length))
            {
                global::XUiC_ItemStack xuiC_ItemStack = (global::XUiC_ItemStack)itemStackControllers[num];
                if (!xuiC_ItemStack.StackLock && (!(xuiC_ItemStack is XUiC_ItemStack quartzItemStack) || !quartzItemStack.IsALockedSlot))
                {
                    ItemStack itemStack = xuiC_ItemStack.ItemStack;
                    if (!xuiC_ItemStack.ItemStack.IsEmpty())
                    {
                        int count = itemStack.count;
                        dstInventory.TryStackItem(0, itemStack);
                        if (itemStack.count > 0
                            && (moveKind == XUiM_LootContainer.EItemMoveKind.All || (moveKind == XUiM_LootContainer.EItemMoveKind.FillAndCreate && dstInventory.HasItem(itemStack.itemValue)))
                            && dstInventory.AddItem(itemStack))
                        {
                            itemStack = ItemStack.Empty.Clone();
                        }
                        if (itemStack.count == 0)
                        {
                            itemStack = ItemStack.Empty.Clone();
                        }
                        else
                        {
                            item = false;
                        }
                        if (count != itemStack.count)
                        {
                            xuiC_ItemStack.ForceSetItemStack(itemStack);
                            item2 = true;
                        }
                    }
                }
                num = (startBottomRight ? (num - 1) : (num + 1));
            }

            return (item, item2);
        }

        private void ClearEventHandlers(XUiController controller, string eventName)
        {
            Type type = typeof(XUiController);
            EventInfo eventInfo = type.GetEvent(eventName);
            FieldInfo eventFieldInfo = AccessTools.Field(type, eventName);

            if (eventInfo == null || eventFieldInfo == null)
            {
                return;
            }

            if (eventFieldInfo.GetValue(controller) is Delegate eventDelegate)
            {
                foreach (var d in eventDelegate.GetInvocationList())
                {
                    eventInfo.RemoveEventHandler(controller, d);
                }
            }
        }
    }
}

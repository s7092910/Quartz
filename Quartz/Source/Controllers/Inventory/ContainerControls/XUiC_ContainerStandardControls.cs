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
        protected static float lastStashTime;

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


        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch(bindingName)
            {
                case "totallockedslotscount":
                    value = inventory != null ? inventory.TotalLockedSlotsCount().ToString() : "0";
                    return true;
                case "combolockedslots":
                    value = comboBox != null ? comboBox.valueText : "0";
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

        public void ChangeLockedSlots(long newValue)
        {
            RefreshBindings();
        }

        protected virtual void MoveSmart(XUiController sender, int mouseButton)
        {

            XUiController srcWindow;
            XUiC_ItemStackGrid srcGrid;
            IInventory dstInventory;
            if (MoveAllowed(out srcWindow, out srcGrid, out dstInventory))
            {
                XUiM_LootContainer.StashItems(srcWindow, srcGrid, dstInventory, 0, inventory.GetLockSlots(), XUiM_LootContainer.EItemMoveKind.FillAndCreate, MoveStartBottomRight);
            }
        }

        protected virtual void MoveFillStacks(XUiController sender, int mouseButton)
        {

            XUiController srcWindow;
            XUiC_ItemStackGrid srcGrid;
            IInventory dstInventory;
            if (MoveAllowed(out srcWindow, out srcGrid, out dstInventory))
            {
                XUiM_LootContainer.StashItems(srcWindow, srcGrid, dstInventory, 0, inventory.GetLockSlots(), XUiM_LootContainer.EItemMoveKind.FillOnly, MoveStartBottomRight);
            }
        }

        protected virtual void MoveFillAndSmart(XUiController sender, int mouseButton)
        {

            XUiController srcWindow;
            XUiC_ItemStackGrid srcGrid;
            IInventory dstInventory;

            float unscaledTime = Time.unscaledTime;
            XUiM_LootContainer.EItemMoveKind moveKind = XUiM_LootContainer.EItemMoveKind.FillOnlyFirstCreateSecond;
            if (unscaledTime - lastStashTime < 2f)
            {
                moveKind = XUiM_LootContainer.EItemMoveKind.FillAndCreate;
            }

            if (MoveAllowed(out srcWindow, out srcGrid, out dstInventory))
            {
                XUiM_LootContainer.StashItems(srcWindow, srcGrid, dstInventory, 0, inventory.GetLockSlots(), moveKind, MoveStartBottomRight);
                lastStashTime = unscaledTime;
            }
        }

        protected virtual void MoveAll(XUiController sender, int mouseButton)
        {
            XUiController srcWindow;
            XUiC_ItemStackGrid srcGrid;
            IInventory dstInventory;
            if (MoveAllowed(out srcWindow, out srcGrid, out dstInventory))
            {
                ValueTuple<bool, bool> valueTuple = XUiM_LootContainer.StashItems(srcWindow, srcGrid, dstInventory, 0, inventory.GetLockSlots(), XUiM_LootContainer.EItemMoveKind.All, MoveStartBottomRight);
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

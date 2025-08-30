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
                    value = inventory != null && inventory.HasLockSlotSupport() ? inventory.TotalLockedSlotsCount().ToString() : "";
                    return true;
                case "individuallockedslotscount":
                    value = inventory != null && inventory.HasLockSlotSupport() ? inventory.IndividualLockedSlotsCount().ToString() : "";
                    return true;
                case "unlockedslotscount":
                    value = inventory != null && inventory.HasLockSlotSupport() ? inventory.UnlockedSlotCount().ToString() : "";
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

            base.MoveSmart();
        }

        protected virtual void MoveFillStacks(XUiController sender, int mouseButton)
        {
            base.MoveFillStacks();
        }

        protected virtual void MoveFillAndSmart(XUiController sender, int mouseButton)
        {
            base.MoveFillAndSmart();
        }

        protected virtual void MoveAll(XUiController sender, int mouseButton)
        {
            base.MoveAll();
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

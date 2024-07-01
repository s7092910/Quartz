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
    public class XUiC_WorkstationFuelGrid : global::XUiC_WorkstationFuelGrid
    {
        private const string TAG = "WorkstationFuelGrid";

        protected string requiredFuels;

        private XUiC_WorkstationWindowGroup workstationGroup;

        private bool updateBackend = false;

        public override void Init()
        {
            base.Init();
            workstationGroup = GetParentByType<XUiC_WorkstationWindowGroup>();
        }

        public override void Update(float _dt)
        {
            //if (string.IsNullOrEmpty(requiredFuels))
            //{
            //    base.Update(_dt);
            //    return;
            //}

            XUiControllerPatch.Update(this, _dt);

            if (workstationData == null)
            {
                return;
            }

            if (ShouldCycleStacks())
            {
                CycleStacks();
                if (updateBackend)
                {
                    UpdateBackend(getUISlots());
                    updateBackend = false;
                }
            }

            if (isOn && (!HasRequirement(null) || workstationData.GetIsBesideWater()))
            {
                TurnOff();
                XUiC_WorkstationFuelGridPatch.onFuelItemsChanged(this);
                return;
            }

            if (isOn)
            {
                bool foundTopItemStack = false;
                for (int i = 0; i < itemControllers.Length; i++)
                {
                    if (!itemControllers[i].ItemStack.IsEmpty() && !foundTopItemStack)
                    {
                        if (itemControllers[i].IsLocked)
                        {
                            itemControllers[i].LockTime = workstationData.GetBurnTimeLeft();
                        }
                        else
                        {
                            itemControllers[i].LockStack(XUiC_ItemStack.LockTypes.Burning, workstationData.GetBurnTimeLeft(), 0, null);
                        }

                        foundTopItemStack = true;
                    }
                    else
                    {
                        itemControllers[i].UnlockStack();
                    }
                }
            }
            else
            {
                for (int i = 0; i < itemControllers.Length; i++)
                {
                    itemControllers[i].UnlockStack();
                }
            }

        }

        public override void OnOpen()
        {
            if (workstationGroup != null)
            {
                TileEntityWorkstation te = workstationGroup.WorkstationData.TileEntity;
                if (te != null)
                {
                    Block block = te.blockValue.Block;

                    requiredFuels = block.Properties.GetString("Workstation.RequiredFuels");

                    string[] fuelNames = requiredFuels.Split(',');
                    for (int i = 0; i < itemControllers.Length; i++)
                    {
                        if (itemControllers[i] is XUiC_RequiredItemStack itemStack)
                        {
                            if (i < fuelNames.Length)
                            {
                                itemStack.RequiredItemClass = ItemClass.GetItemClass(fuelNames[i], false);
                                itemStack.RequiredItemOnly = true;
                            }
                            else
                            {
                                itemStack.RequiredItemClass = null;
                                itemStack.RequiredItemOnly = false;
                            }
                        }
                    }
                }
            }
            base.OnOpen();
        }

        public override void OnClose()
        {
            base.OnClose();
            requiredFuels = string.Empty;
        }

        public override bool HasRequirement(Recipe recipe)
        {
            if (!XUi.IsGameRunning())
            {
                return workstationData.GetBurnTimeLeft() > 0f;
            }
            float num = workstationData.GetBurnTimeLeft();


            for (int i = 0; i < itemControllers.Length; i++)
            {
                global::XUiC_ItemStack xuiC_ItemStack = itemControllers[i];
                if (xuiC_ItemStack != null && !xuiC_ItemStack.ItemStack.IsEmpty())
                {
                    ItemClass itemClass = xuiC_ItemStack.ItemStack.itemValue.ItemClass;
                    if (itemClass != null)
                    {
                        if (!itemClass.IsBlock())
                        {
                            if (itemClass != null && itemClass.FuelValue != null)
                            {
                                num += itemClass.FuelValue.Value;
                            }
                        }
                        else
                        {
                            Block block = Block.list[itemClass.Id];
                            if (block != null)
                            {
                                num += block.FuelValue;
                            }
                        }
                    }
                }
            }
            return num > 0;
        }

        public bool HasRequiredFuels()
        {
            return !string.IsNullOrEmpty(requiredFuels);
        }

        private bool ShouldCycleStacks()
        {
            if (!XUi.IsGameRunning())
            {
                return false;
            }

            int previousFuelValue = 0;
            ItemClass reqItemClass = null;
            for (int i = 0; i < itemControllers.Length; i++)
            {

                int currentFuelValue = 0;
                XUiC_RequiredItemStack xuiC_ItemStack = itemControllers[i] as XUiC_RequiredItemStack;
                if (xuiC_ItemStack != null && !xuiC_ItemStack.ItemStack.IsEmpty())
                {
                    ItemClass itemClass = xuiC_ItemStack.ItemStack.itemValue.ItemClass;
                    if (itemClass != null)
                    {

                        if (!itemClass.IsBlock())
                        {
                            if (itemClass.FuelValue != null)
                            {
                                currentFuelValue = itemClass.FuelValue.Value;
                            }
                        }
                        else
                        {
                            Block block = Block.list[itemClass.Id];
                            if (block != null)
                            {
                                currentFuelValue = block.FuelValue;
                            }
                        }
                    }
                }

                if (xuiC_ItemStack == null)
                {
                    continue;
                }

                if (xuiC_ItemStack.RequiredItemClass != reqItemClass)
                {
                    reqItemClass = xuiC_ItemStack.RequiredItemClass;
                }
                else if (previousFuelValue == 0 && currentFuelValue != 0 && i != 0)
                {
                    return true;
                }

                previousFuelValue = currentFuelValue;
            }
            return false;
        }

        private new void CycleStacks()
        {
            for (int i = itemControllers.Length - 1; i > 0; i--)
            {
                XUiC_RequiredItemStack xuiC_ItemStack = itemControllers[i] as XUiC_RequiredItemStack;
                if (xuiC_ItemStack != null && !xuiC_ItemStack.ItemStack.IsEmpty())
                {
                    XUiC_RequiredItemStack xuiC_ItemStack2 = itemControllers[i - 1] as XUiC_RequiredItemStack;
                    if (xuiC_ItemStack2 != null && xuiC_ItemStack2.ItemStack.IsEmpty() && xuiC_ItemStack.RequiredItemClass == xuiC_ItemStack2.RequiredItemClass)
                    {
                        xuiC_ItemStack2.ItemStack = xuiC_ItemStack.ItemStack.Clone();
                        xuiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
                        updateBackend = true;
                    }
                }
            }
        }
    }
}

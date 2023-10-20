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

using HarmonyLib;

namespace Quartz
{
    public class XUiC_WorkstationToolGrid : global::XUiC_WorkstationToolGrid
    {
        protected string requiredTools;

        private XUiC_WorkstationWindowGroup workstationGroup;

        public override void Init()
        {
            base.Init();
            AccessTools.Field(typeof(global::XUiC_WorkstationToolGrid), "requiredToolsOnly").SetValue(this, true);
            workstationGroup = GetParentByType<XUiC_WorkstationWindowGroup>();
        }

        public override void OnOpen()
        {
            if(workstationGroup != null)
            {
                TileEntityWorkstation te = workstationGroup.WorkstationData.TileEntity;
                if(te != null)
                {
                    Block block = te.blockValue.Block;

                    requiredTools = block.Properties.GetString("Workstation.RequiredTools");

                    string[] toolNames = requiredTools.Split(',');
                    for (int i = 0; i < itemControllers.Length; i++)
                    {
                        if (itemControllers[i] is XUiC_RequiredItemStack itemStack)
                        {
                            if (i < toolNames.Length)
                            {
                                itemStack.RequiredItemClass = ItemClass.GetItemClass(toolNames[i], false);
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
    }
}

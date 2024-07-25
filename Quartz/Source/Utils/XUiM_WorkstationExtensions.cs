/*Copyright 2024 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using UnityEngine;

namespace Quartz.Utils
{
    public static class XUiM_WorkstationExtensions
    {
        public static float GetSlotTotalSmeltTime(this XUiM_Workstation workstation, int slotId)
        {
            TileEntityWorkstation tileEntity = workstation.tileEntity;
            string[] materialNames = tileEntity.MaterialNames;
            ItemStack[] input = tileEntity.Input;
            ItemStack[] tools = tileEntity.Tools;
            ItemClass forId = ItemClass.GetForId(input[slotId].itemValue.type);
            float _originalValue = 0.0f;
            if (forId != null)
            {
                for (int index1 = 0; index1 < materialNames.Length; ++index1)
                {
                    if (forId.MadeOfMaterial.ForgeCategory != null && forId.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(materialNames[index1]))
                    {
                        ItemClass itemClass = ItemClass.GetItemClass("unit_" + materialNames[index1]);
                        if (itemClass != null && itemClass.MadeOfMaterial.ForgeCategory != null)
                        {
                            _originalValue = forId.GetWeight() * (forId.MeltTimePerUnit > 0.0 ? forId.MeltTimePerUnit : 1f);
                            if (tileEntity.isModuleUsed[0])
                            {
                                for (int index2 = 0; index2 < tools.Length; ++index2)
                                {
                                    float _perc_value = 1f;
                                    tools[index2].itemValue.ModifyValue(null, null, PassiveEffects.CraftingOutputCount, ref _originalValue, ref _perc_value, FastTags<TagGroup.Global>.Parse(forId.Name));
                                    _originalValue *= _perc_value;
                                }
                            }
                        }
                    }
                }
            }
            return (float)((_originalValue <= 0.0 || input[slotId].count < 1 ? _originalValue : _originalValue * (input[slotId].count - 1)) + tileEntity.currentMeltTimesLeft[slotId] + 0.95f);
        }

        public static float GetMaxSmeltTime(this XUiM_Workstation workstation)
        {
            float a = 0.0f;
            TileEntityWorkstation tileEntity = workstation.tileEntity;
            for (int slotId = 0; slotId < tileEntity.currentMeltTimesLeft.Length; ++slotId)
                a = Mathf.Max(a, workstation.GetSlotTotalSmeltTime(slotId));
            return a;
        }
    }
}
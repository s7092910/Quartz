/*Copyright 2021 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

namespace Quartz.PlayerStats
{

    public class BagUsedSlots : StatBinding
    {
        public BagUsedSlots(string name) : base(name)
        {
        }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return player.bag.GetUsedSlotCount().ToString();
        }
    }

    public class BagCarryCapacity : StatBinding
    {
        public BagCarryCapacity(string name) : base(name)
        {
        }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return MathUtils.Min(player.bag.MaxItemCount, player.bag.SlotCount).ToString();
        }
    }

    public class BagMaxCarryCapacity : StatBinding
    {
        public BagMaxCarryCapacity(string name) : base(name)
        {
        }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return player.bag.MaxItemCount.ToString();
        }
    }

    public class BagSize : StatBinding
    {
        public BagSize(string name) : base(name)
        {
        }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return player.bag.SlotCount.ToString();
        }
    }
}

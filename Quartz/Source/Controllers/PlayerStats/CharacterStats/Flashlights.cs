/*Copyright 2026 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using System.Collections.Generic;

namespace Quartz.PlayerStats
{
    public class FlashLight : StatBinding
    {
        protected int flashlightId;
        protected int gunFlashlightId;
        protected int helmetFlashlightId;

        private bool initialized = false;

        public FlashLight(string name) : base(name)
        {

        }

        public override string GetCurrentValue(EntityPlayer player)
        {
            if (!initialized)
                init();

            List<ItemValue> activatedableItemPool = player.GetActivatableItemPool();
            foreach (ItemValue item in activatedableItemPool)
            {
                if (item.Activated > 0)
                {
                    int itemId = item.ItemClass.Id;

                    if (checkId(itemId))
                    {
                        return "true";
                    }
                }
            }
            return "false";
        }

        private void init()
        {
            flashlightId = ItemClass.GetItemClass("meleeToolFlashlight02").Id;
            gunFlashlightId = ItemClass.GetItemClass("modGunFlashlight").Id;
            helmetFlashlightId = ItemClass.GetItemClass("modArmorHelmetLight").Id;
            initialized = true;
        }

        public virtual bool checkId(int itemId)
        {
            return itemId == flashlightId || itemId == gunFlashlightId || itemId == helmetFlashlightId;
        }
    }

    public class HandFlashLight : FlashLight
    {
        public HandFlashLight(string name) : base(name)
        {
        }

        public override bool checkId(int itemId)
        {
            return itemId == flashlightId;
        }
    }

    public class GunFlashLight : FlashLight
    {
        public GunFlashLight(string name) : base(name)
        {
        }

        public override bool checkId(int itemId)
        {
            return itemId == gunFlashlightId;
        }
    }

    public class HelmetFlashLight : FlashLight
    {
        public HelmetFlashLight(string name) : base(name)
        {
        }

        public override bool checkId(int itemId)
        {
            return itemId == helmetFlashlightId;
        }
    }
}

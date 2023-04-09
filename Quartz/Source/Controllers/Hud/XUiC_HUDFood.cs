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

using Quartz.Hud;

namespace Quartz
{
    public class XUiC_HUDFood : XUiC_HUDStat
    {
        protected override string GetStatName()
        {
            return "food";
        }

        protected override float GetCurrentStat()
        {
            return LocalPlayer.Stats.Food.Value;
        }

        protected override float GetMaxStat()
        {
            return LocalPlayer.Stats.Food.Max;
        }

        protected override float GetModifiedMax()
        {
            return LocalPlayer.Stats.Food.ModifiedMax;
        }

        protected override float GetStatUIPercentage()
        {
            return LocalPlayer.Stats.Food.ValuePercentUI;
        }
    }
}

/*Licensed under the Apache License, Version 2.0 (the "License");
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
    public class CharacterLevel : StatBinding
    {
        public CharacterLevel(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return player.Progression.GetLevel().ToString();
        }

    }

    public class CharacterLootStage : StatBinding
    {
        public CharacterLootStage(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return player.GetHighestPartyLootStage(0f, 0f).ToString();
        }

    }

    public class CharacterCoreTemp : StatBinding
    {
        public CharacterCoreTemp(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return XUiM_Player.GetCoreTemp(player);
        }

    }

    public class CharacterZombieKills : StatBinding
    {
        public CharacterZombieKills(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return XUiM_Player.GetZombieKills(player).ToString();
        }

    }

    public class CharacterPlayerKills : StatBinding
    {
        public CharacterPlayerKills(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return XUiM_Player.GetPlayerKills(player).ToString();
        }

    }

    public class CharacterDeaths : StatBinding
    {
        public CharacterDeaths(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return XUiM_Player.GetDeaths(player).ToString();
        }

    }

    public class CharacterTravelled : StatBinding
    {
        public CharacterTravelled(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return XUiM_Player.GetKMTraveled(player);
        }

    }


    public class CharacterItemsCrafted : StatBinding
    {
        public CharacterItemsCrafted(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return XUiM_Player.GetItemsCrafted(player).ToString();
        }

    }

    public class CharacterLongestLife : StatBinding
    {
        public CharacterLongestLife(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return XUiM_Player.GetLongestLife(player);
        }

    }


    public class CharacterCurrentLife : StatBinding
    {
        public CharacterCurrentLife(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return XUiM_Player.GetCurrentLife(player);
        }

    }


    public class CharacterXPToNextLevel : StatBinding
    {
        public CharacterXPToNextLevel(string name) : base(name) { }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return XUiM_Player.GetXPToNextLevel(player).ToString();
        }

    }
}
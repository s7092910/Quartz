using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quartz.PlayerStats
{

    public static class PlayerStatBindings
    {

        private const string TAG = "PlayerStatBindings";
        private static readonly Dictionary<string, StatBinding> supportedBindings = new Dictionary<string, StatBinding>();

        //Adds all the base supported bindings
        static PlayerStatBindings()
        {
            AddNewBinding(new CharacterLevel("PlayerLevel"));
            AddNewBinding(new CharacterLootStage("PlayerLootStage"));
            AddNewBinding(new CharacterCoreTemp("PlayerCoreTemp"));
            AddNewBinding(new CharacterZombieKills("PlayerZombieKills"));
            AddNewBinding(new CharacterPlayerKills("PlayerPvPKills"));
            AddNewBinding(new CharacterDeaths("PlayerDeaths"));
            AddNewBinding(new CharacterTravelled("PlayerTravelled"));
            AddNewBinding(new CharacterItemsCrafted("PlayerItemsCrafted"));
            AddNewBinding(new CharacterLongestLife("PlayerLongestLife"));
            AddNewBinding(new CharacterCurrentLife("PlayerCurrentLife"));
            AddNewBinding(new CharacterXPToNextLevel("PlayerXPToNextLevel"));

            AddNewBinding(new CharacterDisplayInfoStat("PlayerHealthModifier", 0));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerStaminaModifier", 1));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerArmor", 2));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerExplosionResist", 3));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerCritResist", 4));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerStaminaRegen", 5));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerHealthRegen", 6));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerMedicalHealthRegen", 7));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerColdResist", 8));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerHeatResist", 9));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerMobility", 10));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerJumpStrength", 11));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerCarryingCapacity", 12));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerDamage", 13));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerBlockDamage", 14));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerRPM", 15));
            AddNewBinding(new CharacterDisplayInfoStat("PlayerAPM", 16));

            AddNewBinding(new FlashLight("InventoryIsFlashLightOn"));
            AddNewBinding(new HandFlashLight("InventoryIsHandFlashLightOn"));
            AddNewBinding(new GunFlashLight("InventoryIsGunFlashLightOn"));
            AddNewBinding(new HelmetFlashLight("InventoryIsHelmetFlashLightOn"));

            AddNewBinding(new BagUsedSlots("PlayerBagUsedSlots"));
            AddNewBinding(new BagSize("PlayerBagSize"));
            AddNewBinding(new BagCarryCapacity("PlayerCarryCapacity"));
            AddNewBinding(new BagMaxCarryCapacity("PlayerMaxCarryCapacity"));
        }

        /// <summary>
        /// Finds the BindingType that contains the bindingName <br/>
        /// An error will be logged to the 7 Days to Die console if a <see cref="Binding"/> can not be found for the bindingName
        /// </summary>
        /// <param name="name">The bindingName to search for</param>
        /// <returns>The found <see cref="Binding"/>, otherwise <see langword="null"/></returns>
        public static StatBinding GetBinding(string bindingName)
        {
            if (supportedBindings.TryGetValue(bindingName.ToLower(), out StatBinding binding))
            {
                return binding;
            }

            var message = string.Format("{0} is not a supported bindingName", bindingName);
            Logging.Error(TAG, message);

            return null;
        }

        /// <summary>
        /// Determines if there is a <see cref="Binding"/> for the bindingName.<br/>
        /// An error will be logged to the 7 Days to Die console if the bindingName doesn't have a corresponding <see cref="Binding"/>
        /// </summary>
        /// <param name="bindingName">The bindingName to check for a corresponding Binding </param>
        /// <returns><see langword="true"/> if the bindingName has a Binding, otherwise <see langword="false"/> </returns>
        public static bool SupportsBinding(string bindingName)
        {
            if (!supportedBindings.ContainsKey(bindingName.ToLower()))
            {
                var message = string.Format("{0} is not a supported bindingName", bindingName);
                Logging.Error(TAG, message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds a new <see cref="Binding"/> to the Supported <see cref="Binding"/>.
        /// <para>
        /// This method must be called to add a new custom <see cref="Binding"/> that is not already supported <br/>
        /// if a custom binding is wanted to be used. See the Custom Binding Example to see how to <br/>
        /// add a new custom <see cref="Binding"/>
        /// </para>
        /// </summary>
        /// <param name="binding"></param>
        public static void AddNewBinding(StatBinding binding)
        {
            supportedBindings.Add(binding.Name, binding);
        }
    }

    public abstract class StatBinding
    {
        public string Name { get; set; }

        /// <summary>
        /// The <see cref="Binding"/> constructor
        /// </summary>
        /// <param name="value">The <see langword="int"/> value used for identifying the <see cref="Binding"/></param>
        /// <param name="name">The bindingName for the <see cref="Binding"/></param>
        protected StatBinding(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the current value of the <see cref="Binding"/> to bind to the XML
        /// </summary>
        /// <param name="player">The <see cref="EntityPlayer">player</see> the stat value belongs to</param>
        /// <returns>The value to bind to the XML</returns>
        public abstract string GetCurrentValue(EntityPlayer player);

        /// <summary>
        /// Determines if the value for the <see cref="Binding"/> has been updated and sets the last value in if it has been updated
        /// </summary>
        /// <param name="player">The <see cref="EntityPlayer">player</see> the stat value belongs to</param>
        /// <param name="lastValue">The value for the <see cref="Binding"/> since it was last checked to see if it has been updated<br/>
        /// If last value has been found to be changed, the new updated value will be sent back in this <see langword="ref"/></param>
        /// <returns><see langword="true"/> if the value has changed, otherwise <see langword="false"/></returns>
        public virtual bool HasValueChanged(EntityPlayer player, ref string lastValue)
        {
            string currentValue = GetCurrentValue(player);
            bool changed = lastValue != currentValue;
            lastValue = currentValue;
            return changed;
        }
    }
}

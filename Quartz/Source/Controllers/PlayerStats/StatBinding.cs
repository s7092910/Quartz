using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz
{
    public abstract class StatBinding
    {
        /// <summary>
        /// The <see cref="Binding"/> constructor
        /// </summary>
        /// <param name="value">The <see langword="int"/> value used for identifying the <see cref="Binding"/></param>
        /// <param name="name">The bindingName for the <see cref="Binding"/></param>
        protected StatBinding(string name)
        {
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

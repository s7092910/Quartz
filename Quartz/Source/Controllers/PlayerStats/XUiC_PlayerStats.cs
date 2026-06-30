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

using Quartz.PlayerStats;
using System.Collections.Generic;

namespace Quartz
{
    internal class XUiC_PlayerStats : XUiController
    {
        private string TAG = "PlayerStats";

        public bool isDirty;

        private EntityPlayer localPlayer;
        private List<StatBinding> activeBindings = new List<StatBinding>();
        private Dictionary<StatBinding, string> bindingsLastValue = new Dictionary<StatBinding, string>();
        public override void Init()
        {
            base.Init();
            this.isDirty = true;
        }

        /// <summary>
        /// Determines if the values have changed related to the bindings for this controller. If the values<br/>
        /// have changed or if the UI is dirty, it will give a call to get the new values for each binding.
        /// </summary>
        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (localPlayer == null && XUi.IsGameRunning())
            {
                localPlayer = base.xui.playerUI.entityPlayer;
            }
            if (isDirty || HasChanged())
            {
                Logging.Out(TAG, "isDirty = " + isDirty + ", localPlayer = " + localPlayer);
                if (localPlayer == null)
                {
                    return;
                }
                base.RefreshBindings();
                isDirty = false;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            isDirty = true;
        }

        public override void OnClose()
        {
            base.OnClose();
        }

        /// <summary>
        /// Gets the value related to the BindingName.
        /// </summary>
        /// <param name="value">The returned string that contains the value to be used in the xml for the binding</param>
        /// <param name="bindingName">The bindingName in the xml</param>
        /// <returns><see langword="true"/> if the value has been found or has a <see cref="BindingType">, otherwise <see langword="false"/></returns>
        public override bool GetBindingValueInternal(ref string value, string bindingName)
        {
            Logging.Out(TAG, "GetBindingValue(), bindingName = " + bindingName);
            if (bindingName != null)
            {
                if (localPlayer != null)
                {
                    value = GetBindingValue(bindingName);
                    Logging.Out(TAG, "GetBindingValue(), bindingValue = " + value);
                }
                else
                {
                    value = "";
                }

                //Checks to see if value is empty or null. If it is, it will check to see if there is a supported Binding
                //for the bindingName, it will return false if no BindingType is found.
                if (string.IsNullOrEmpty(value))
                {
                    return PlayerStatBindings.SupportsBinding(bindingName);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if any of the active <see cref="BindingType"> has their binding value updated.
        /// </summary>
        /// <returns><see langword="true"/> if any binding value has been updated</returns>
        private bool HasChanged()
        {
            foreach (StatBinding stat in activeBindings)
            {
                string lastValue = bindingsLastValue[stat];
                if (stat.HasValueChanged(localPlayer, ref lastValue))
                {
                    bindingsLastValue[stat] = lastValue;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Searches the activeBindings for the corresponding <see cref="BindingType"/>. If the <see cref="BindingType"/> is not found <br/>
        /// in the activeBindings, searches the Supported <see cref="BindingType">BindingTypes</see> by looking up the bindingName.
        /// <para>
        /// If a <see cref="BindingType"/> has been found, it will fetch the current binding value
        /// </para>
        /// </summary>
        /// <param name="bindingName">the bindingName in the xml and the name of the <see cref="BindingType"/></param>
        /// <returns>The value for the binding</returns>
        private string GetBindingValue(string bindingName)
        {
            StatBinding stat = activeBindings.Find(f => f.Name.EqualsCaseInsensitive(bindingName));
            if (stat == null)
            {
                stat = PlayerStatBindings.GetBinding(bindingName);
                if (stat == null)
                {
                    return "";
                }
                activeBindings.Add(stat);
                bindingsLastValue.Add(stat, "");
                Logging.Inform(TAG, "Adding binding with bindingName = " + bindingName);
            }

            return stat.GetCurrentValue(localPlayer);
        }
    }
}

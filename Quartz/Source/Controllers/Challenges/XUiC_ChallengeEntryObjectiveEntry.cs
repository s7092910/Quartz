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

using Challenges;
using Quartz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz
{
    public class XUiC_ChallengeEntryObjectiveEntry : XUiController
    {
        private BaseChallengeObjective objective;

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (IsDirty)
            {
                RefreshBindings();
                IsDirty = false;
            }
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "hasobjective":
                    value = HasObjective().ToString();
                    return true;
                case "objective":
                    value = GetObjectiveText();
                    return true;
                case "objectivefill":
                    value = GetObjectiveFill();
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        public void SetEntry(BaseChallengeObjective objective)
        {
            if (this.objective != objective)
            {
                this.objective = objective;
                IsDirty = true;
            }
        }

        public void clearEntry()
        {
            objective = null;
            IsDirty = true;
        }

        private bool HasObjective()
        {
            return objective != null;
        }

        private string GetObjectiveText()
        {
            if (objective != null)
            {
                return objective.ObjectiveText;
            }

            return "";
        }

        private string GetObjectiveFill()
        {
            if (objective != null)
            {
                return objective.FillAmount.ToString();
            }

            return "0";
        }

    }
}

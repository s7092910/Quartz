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

namespace Quartz
{
    public class XUiC_ChallengeEntryDescriptionWindow : global::XUiC_ChallengeEntryDescriptionWindow
    {
        private XUiC_ChallengeEntryObjectiveEntry[] objectiveControllers;

        public override void Init()
        {
            base.Init();

            objectiveControllers = GetChildrenByType<XUiC_ChallengeEntryObjectiveEntry>();
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "objectivecount":
                    value = currentChallenge != null ? currentChallenge.ObjectiveList.Count.ToString() : "0";
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        public void SetObjectives()
        {
            for (int i = 0; i < objectiveControllers.Length; i++)
            {
                if (currentChallenge != null && i < currentChallenge.ObjectiveList.Count)
                {
                    objectiveControllers[i].SetEntry(currentChallenge.ObjectiveList[i]);
                }
                else
                {
                    objectiveControllers[i].clearEntry();
                }
            }
        }
    }
}

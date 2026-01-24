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

namespace Quartz
{
    public class XUiC_AssembleWindow : global::XUiC_AssembleWindow
    {
        private XUiC_AssembleItemStatEntry[] itemStatControllers;

        public override void Init()
        {
            base.Init();

            itemStatControllers = GetChildrenByType<XUiC_AssembleItemStatEntry>();
        }

        public override bool GetBindingValueInternal(ref string value, string bindingName)
        {
            if (bindingName.StartsWith("itemstat") && bindingName.Contains("-"))
            {
                string[] split = bindingName.Split('-');
                int index = 0;
                if (split.Length == 2)
                {
                    if (int.TryParse(split[1], out index))
                    {
                        index--;
                    }
                }
                switch (split[0])
                {
                    case "itemstattitle":
                        value = GetStatTitle(index);
                        return true;
                    case "itemstaticon":
                        value = GetStatIcon(index);
                        return true;
                    case "itemstat":
                        value = GetStatValue(index);
                        return true;
                    default:
                        return base.GetBindingValueInternal(ref value, bindingName);
                }
            }

            return base.GetBindingValueInternal(ref value, bindingName);
        }

        public void SetItemStats()
        {
            if(itemStack == null || itemStack.IsEmpty())
            {
                return;
            }

            for (int i = 0; i < itemStatControllers.Length; i++)
            {
                if (itemDisplayEntry != null && i < itemDisplayEntry.DisplayStats.Count)
                {
                    itemStatControllers[i].SetEntry(itemStack, itemDisplayEntry.DisplayStats[i]);
                }
                else
                {
                    itemStatControllers[i].clearEntry();
                }
            }
        }

        private string GetStatIcon(int index)
        {
            if (itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
            {
                return string.Empty;
            }

            if (itemDisplayEntry.DisplayStats[index] is Models.DisplayInfoEntry displayInfoEntry)
            {
                return displayInfoEntry.icon;
            }
            return string.Empty;
        }
    }
}

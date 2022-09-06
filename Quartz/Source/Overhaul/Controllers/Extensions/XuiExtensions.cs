/*Copyright 2022 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

namespace QuartzOverhaul.Extensions
{
    public static class XuiExtensions
    {

        public static T GetChildByInterface<T>(this XUi xui) where T : class
        {
            for (int i = 0; i < xui.WindowGroups.Count; i++)
            {
                if(xui.WindowGroups[i].Controller is XUiBaseController child)
                {
                    T childByType = child.GetChildByInterface<T>();
                    if (childByType != null)
                    {
                        return childByType;
                    }
                }
            }

            return null;
        }
    }
}

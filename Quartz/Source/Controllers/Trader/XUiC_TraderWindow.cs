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
    public class XUiC_TraderWindow : global::XUiC_TraderWindow
    {
        private string categoryDisplayName = "";
        private string categorySpriteName = "";

        private string generalStock = Localization.Get("xuiGeneralStock");
        private string secretStash = Localization.Get("xuiSecretStash");
        private string emptyVendingMachine = Localization.Get("xuiEmptyVendingMachine");
        private string ownedVendingMachine = Localization.Get("xuiVendingWithOwner");

        [XuiXmlBinding("selectedcategoryname")]
        public string CategoryDisplayName { get => categoryDisplayName; }

        [XuiXmlBinding("selectedcategorysprite")]
        public string CategorySpriteName { get => categorySpriteName; }

        public override void Init()
        {
            base.Init();
            categoryList = windowGroup.Controller.GetChildByType<XUiC_CategoryList>();
            categoryList.CategoryChanged += HandleCategoryChanged;

        }

        public new void HandleCategoryChanged(XUiC_CategoryEntry categoryEntry)
        { 
            string text = categoryEntry.CategoryName;
            if (text == "SECRET STASH")
            {
                categoryDisplayName = "";
                categorySpriteName = "ui_game_symbol_map_trader";
                isSecretStash = true;
            }
            else
            {
                categoryDisplayName = categoryEntry.CategoryDisplayName;
                categorySpriteName = categoryEntry.SpriteName;
                isSecretStash = false;
            }

            RefreshBindings();
        }

        [XuiXmlBinding("headername")]
        private string GetHeaderText()
        {
            if (IsVending)
            {
                if (IsRentable || IsPlayerOwned)
                {
                    PlatformUserIdentifierAbs owner = CurrentVending.GetOwner();
                    if (owner != null)
                    {
                        string playerName = GameManager.Instance.persistentPlayers.GetPlayerData(owner).PlayerName.DisplayName;
                        return string.Format(ownedVendingMachine, playerName);
                    }
                    else
                    {
                        return emptyVendingMachine;
                    }
                }
                else
                {
                    return generalStock;
                }
            }
            else
            {
                return isSecretStash ? secretStash : generalStock;
            }
        }
    }
}

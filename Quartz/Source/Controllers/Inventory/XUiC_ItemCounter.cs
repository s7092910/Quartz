using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz
{
    public class XUiC_ItemCounter : XUiController
    {
        protected enum Location
        {
            Bag,
            Toolbelt,
            Both
        }

        private int count;

        private ItemValue itemValue = ItemValue.None.Clone();
        private Location location;

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (!XUi.IsGameRunning() || !ViewComponent.IsVisible || itemValue.IsEmpty())
            {
                return;
            }

            int newCount = 0;
            switch (location)
            {
                case Location.Bag:
                    newCount = xui.PlayerInventory.Backpack.GetItemCount(itemValue);
                    break;
                case Location.Toolbelt:
                    newCount = xui.PlayerInventory.Toolbelt.GetItemCount(itemValue);
                    break;
                case Location.Both:
                    newCount = xui.PlayerInventory.Backpack.GetItemCount(itemValue) + xui.PlayerInventory.Toolbelt.GetItemCount(itemValue);
                    break;
            }

            if (newCount != count)
            {
                count = newCount;
                RefreshBindings();
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            RefreshBindings();
        }

        public override bool GetBindingValueInternal(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "itemcount":
                    value = count.ToString();
                    return true;
                default:
                    return base.GetBindingValueInternal(ref value, bindingName);
            }
        }

        [XuiXmlAttribute("location")]
        private void ParseLocation(string location)
        {
            this.location = EnumUtils.Parse<Location>(location, true);
        }

        [XuiXmlAttribute("itemname")]
        private void ParseItemName(string itemName)
        {
            itemValue = ItemClass.GetItem(itemName, true);
        }

    }
}

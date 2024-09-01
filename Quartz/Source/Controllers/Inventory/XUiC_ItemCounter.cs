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

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "itemcount":
                    value = count.ToString();
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }

        public override bool ParseAttribute(string name, string value, XUiController parent)
        {
            switch (name)
            {
                case "location":
                    location = EnumUtils.Parse<Location>(value, true);
                    return true;
                case "itemname":
                    itemValue = ItemClass.GetItem(value, true);
                    return true;
                default:
                    return base.ParseAttribute(name, value, parent);
            }
        }

    }
}

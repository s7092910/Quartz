using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz
{
    public class XUiC_RecipeEntry : global::XUiC_RecipeEntry
    {

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch(bindingName)
            {
                case "workstationname":
                    value = Recipe != null ? Localization.Get(Recipe.craftingArea): "";
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }
    }
}

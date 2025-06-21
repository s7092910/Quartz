using Quartz.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz
{
    public static class NavObjectExtensions
    {
        public static bool IsOnMiniMap(this NavObject instance)
        {
            NavObjectMapSettings mapSettings = instance.CurrentMapSettings;

            if (mapSettings == null)
            {
                return false;
            }

            if (!mapSettings.Properties.Contains("minimap_only"))
            {
                return true;
            }

            return MinimapSettings.ShowMinimapOnlyIcons || !mapSettings.Properties.GetBool("minimap_only");
        }

        public static bool IsOnMap(this NavObject instance)
        {
            NavObjectMapSettings mapSettings = instance.CurrentMapSettings;

            return mapSettings != null && !(mapSettings.Properties.Contains("minimap_only") && mapSettings.Properties.GetBool("minimap_only"));
        }
    }
}

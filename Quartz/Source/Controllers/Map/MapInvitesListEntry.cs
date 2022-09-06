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

namespace Quartz
{
    public class MapInvitesListEntry : global::XUiC_MapInvitesListEntry 
    {
        private const string TAG = "MapInvitesListEntry";

        protected Waypoint cachedWaypoint;

        protected Waypoint CachedWaypoint
        {
            set
            {
                if (cachedWaypoint != value)
                {
                    cachedWaypoint = value;
                    IsDirty = true;
                }
            }
        }
        public override void Update(float _dt)
        {
            base.Update(_dt);
            CachedWaypoint = Waypoint;
            if (IsDirty)
            {
                RefreshBindings();
                IsDirty = false;
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            IsDirty = true;
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            switch (bindingName)
            {
                case "isempty":
                    value = Waypoint == null ? "true" : "false";
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
        }
    }
}

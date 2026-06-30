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

using Quartz.Utils;
using System;

namespace Quartz
{
    public class XUiC_WorkstationWindowGroup : global::XUiC_WorkstationWindowGroup
    {
        private bool showTotalBurnTime = false;
        private bool showMaxSmeltTime = false;

        private bool showHoursInBurnTime;
        private bool showHoursInSmeltTimes;

        private XUiV_Label smeltTimeLeft;

        [XuiXmlAttribute("fueltimeinhours")]
        public bool ShowHoursInBurnTime { get => showHoursInBurnTime; set => showHoursInBurnTime = value; }

        [XuiXmlAttribute("smelttimeinhours")]
        public bool ShowHoursInSmeltTimes { get => showHoursInSmeltTimes; set => showHoursInSmeltTimes = value; }

        public override void Init()
        {
            base.Init();
            object showHours;

            XUiController childById = GetChildById("smeltTimeLeft");
            if (childById != null)
            {
                if(childById.CustomAttributes.TryGetValue("smelttimeinhours",out showHours) && showHours is bool)
                {
                    showHoursInSmeltTimes = (bool)showHours;
                }
                smeltTimeLeft = childById.ViewComponent as XUiV_Label;
            }

            if (burnTimeLeft != null && burnTimeLeft.Controller.CustomAttributes.TryGetValue("fueltimeinhours", out showHours) && showHours is bool)
            {
                showHoursInBurnTime = (bool)showHours;
            }

            showTotalBurnTime = fuelWindow != null && burnTimeLeft != null;
            showMaxSmeltTime = inputWindow != null && smeltTimeLeft != null;
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (WorkstationData == null)
            {
                return;
            }

            if (showTotalBurnTime && showHoursInBurnTime)
            {
                burnTimeLeft.Text = FormatTimerWithHours(WorkstationData.GetTotalBurnTimeLeft());
            }

            if (showMaxSmeltTime)
            {
                float maxSmeltTime = WorkstationData.GetMaxSmeltTime();
                smeltTimeLeft.Text = showHoursInSmeltTimes ? FormatTimerWithHours(maxSmeltTime): FormatTimerWithoutHours(maxSmeltTime);
            }
        }

        private string FormatTimerWithHours(float timer)
        {
            int num = (int)((double)timer / 3600.0);
            string str1 = num.ToString("0");
            string str2 = ((float)(int)((double)timer % 3600.0) / 60f).ToString("00");
            num = (int)((double)timer % 60.0);
            string str3 = num.ToString("00");
            return string.Format("{0}:{1}:{2}", (object)str1, (object)str2, (object)str3);
        }

        private string FormatTimerWithoutHours(float timer)
        {
            int num = (int)((double)timer / 60.0);
            string str1 = num.ToString("00");
            num = (int)((double)timer % 60.0);
            string str2 = num.ToString("00");
            return string.Format("{0}:{1}", (object)str1, (object)str2);
        }
    }
}
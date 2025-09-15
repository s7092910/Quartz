/*Copyright 2025 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using UnityEngine;
using static UILabel;

namespace Quartz
{
    public class XUiC_SubtitlesDisplay : global::XUiC_SubtitlesDisplay
    {

        public override void Init()
        {
            XUiV_Label subtitlesLabel = GetChildById("lblSubtitle").ViewComponent as XUiV_Label;

            int maxLineCount = 2;
            UILabel.Overflow overflow = Overflow.ShrinkContent;

            if (subtitlesLabel != null) 
            {
                maxLineCount = subtitlesLabel.MaxLineCount;
                overflow = subtitlesLabel.Overflow;
            }
            base.Init();

            if (subtitlesLabel != null)
            {
                subtitlesLabel.MaxLineCount = maxLineCount;
                subtitlesLabel.Overflow = overflow;
            }
        }

        public override void Update(float _dt)
        {
            XUiControllerPatch.Update(this, _dt);

            if (IsOpen)
            {
                if (Time.time - openTime >= duration)
                {
                    xui.playerUI.windowManager.CloseIfOpen("SubtitlesDisplay");
                    XUiC_SubtitlesDisplay.IsDisplaying = false;
                }
            }
        }
    }
}

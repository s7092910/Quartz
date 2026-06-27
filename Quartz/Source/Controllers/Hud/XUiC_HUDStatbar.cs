/*Copyright 2023 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using System;
using UnityEngine;

namespace Quartz.Hud
{
    public abstract class XUiC_HUDStatbar : XUiController
    {
        protected EntityPlayerLocal localPlayer;

        [XuiBindComponent("BarContent", false)]
        private XUiV_Sprite barContent;

        private float cachedFillPercentage;
        private float cachedValue;
        private float smoothTime = 0.3f;
        private float velocity;

        protected readonly CachedStringFormatter<int> statcurrentFormatterInt = new CachedStringFormatter<int>((int _i) => _i.ToString());
        protected readonly CachedStringFormatter<float> statcurrentFormatterFloat = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());
        protected readonly CachedStringFormatter<int, int> statcurrentWMaxFormatterAOfB = new CachedStringFormatter<int, int>((int _i, int _i1) => $"{_i}/{_i1}");
        protected readonly CachedStringFormatter<float, float> statmodifiedmaxFormatter = new CachedStringFormatter<float, float>((float _f1, float _f2) => (_f1 / _f2).ToCultureInvariantString());

        public EntityPlayerLocal LocalPlayer { get => localPlayer; }

        [XuiXmlAttribute("animation_duration")]
        public float AnimationDuration 
        { 
            get => smoothTime; 
            set => smoothTime = value; 
        }

        public override void Init()
        {
            base.Init();
            IsDirty = true;
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (localPlayer == null && XUi.IsGameRunning())
            {
                localPlayer = xui.playerUI.entityPlayer;
                if(localPlayer != null)
                {
                    IsDirty = true;
                    cachedValue = GetCurrentStat();
                    cachedFillPercentage = GetStatUIPercentage();
                }
            }

            RefreshFill();
            if (IsDirty || HasChanged())
            {
                IsDirty = false;
                RefreshBindings();
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            IsDirty = true;
            RefreshBindings();
        }

        public override void OnClose()
        {
            base.OnClose();
        }

        public void RefreshFill()
        {
            if (barContent == null || !IsStatVisible())
            {
                return;
            }

            float b = GetStatUIPercentage();
            if (b == cachedFillPercentage && !IsDirty)
            {
                return;
            }

            cachedFillPercentage = Mathf.SmoothDamp(cachedFillPercentage, b, ref velocity, smoothTime);
            float diff = Math.Abs(b - cachedFillPercentage);
            if ((diff / b) < 0.005)
            {
                cachedFillPercentage = b;
                velocity = 0;
            }

            float fill = Math.Max(cachedFillPercentage, 0f);
            barContent.Fill = fill;
        }

        protected virtual bool HasChanged()
        {
            if(localPlayer == null)
            {
                return false;
            }

            float value = GetCurrentStat();

            bool result = cachedValue != value;
            cachedValue = value;

            return result;
        }

        protected virtual bool IsStatVisible()
        {
            if (localPlayer == null || localPlayer.IsDead() || xui.playerUI.windowManager.IsFullHUDDisabled() 
                || (!xui.DragAndDropWindow.InMenu && xui.playerUI.windowManager.IsHUDPartialHidden()))
            {
                return false;
            }

            return true;
        }

        protected abstract float GetStatUIPercentage();

        protected abstract float GetCurrentStat();

        protected abstract float GetMaxStat();

        protected abstract float GetModifiedMax();

        [XuiXmlBinding("stat")]
        private int GetStatBinding()
        {
            return GetStatCurrentBinding();
        }

        [XuiXmlBinding("statcurrent")]
        private int GetStatCurrentBinding()
        {
            int current = 0;
            if (localPlayer != null)
            {
                current = Mathf.RoundToInt(GetCurrentStat());
            }

            return current;
        }

        [XuiXmlBinding("statmax")]
        private int GetStatMaxBinding()
        {
            int max = 0;
            if (localPlayer != null)
            {
                max = Mathf.RoundToInt(GetMaxStat());
            }

            return max;
        }

        [XuiXmlBinding("statcurrentwithmax")]
        private string GetStatCurrentWithMaxBinding()
        {
            return GetStatWithMaxBinding();
        }

        [XuiXmlBinding("statwithmax")]
        private string GetStatWithMaxBinding()
        {
            string value = "0";
            if (localPlayer != null)
            {
                value = statcurrentWMaxFormatterAOfB.Format(Mathf.RoundToInt(GetCurrentStat()), Mathf.RoundToInt(GetMaxStat()));
            }

            return value;
        }

        [XuiXmlBinding("statmodifiedmax")]
        private float GetStatModifiedMaxBinding()
        {
            float percentage = 0;
            if (localPlayer != null)
            {
                percentage = GetModifiedMax()/GetMaxStat();
            }

            return percentage;
        }

        [XuiXmlBinding("statpercentage")]
        private int GetStatPrecentageBinding()
        {
            int percentage = 0;
            if (localPlayer != null)
            {
                percentage = (int)(GetStatUIPercentage() * 100);
            }

            return percentage;
        }

        [XuiXmlBinding("statuipercentage")]
        private float GetStatUiPrecentageBinding()
        {
            float percentage = 0;
            if (localPlayer != null)
            {
                percentage = GetStatUIPercentage();
            }

            return percentage;
        }

        [XuiXmlBinding("statvisible")]
        private bool IsStatVisibleBinding()
        {
            return IsStatVisible();
        }
    }
}

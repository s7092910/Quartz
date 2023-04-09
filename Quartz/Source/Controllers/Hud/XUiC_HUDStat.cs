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
    public abstract class XUiC_HUDStat : XUiController
    {
        private EntityPlayerLocal localPlayer;

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

        public override void Init()
        {
            base.Init();
            IsDirty = true;
            XUiController childById = GetChildById("BarContent");
            if (childById != null)
            {
                barContent = (XUiV_Sprite)childById.ViewComponent;
            }

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

        public override bool ParseAttribute(string attribute, string value, XUiController _parent)
        {
            switch (attribute)
            {
                case "animation_duration":
                    float.TryParse(value, out smoothTime);
                    return true;
                default:
                    return base.ParseAttribute(attribute, value, _parent);
            }
        }

        public override bool GetBindingValue(ref string value, string bindingName)
        {
            bindingName = bindingName.Replace(GetStatName(), "stat");
            switch (bindingName)
            {
                case "statcurrent":
                    value = "0";
                    if (localPlayer != null)
                    {
                        value = statcurrentFormatterInt.Format((int)GetCurrentStat());
                    }
                    return true;
                case "stat":
                    value = "0";
                    if (localPlayer != null)
                    {
                        value = statcurrentFormatterInt.Format((int)GetCurrentStat());
                    }
                    return true;
                case "statmax":
                    value = "0";
                    if (localPlayer != null)
                    {
                        value = statcurrentFormatterInt.Format((int)GetMaxStat());
                    }
                    return true;
                case "statcurrentwithmax":
                    value = "0";
                    if (localPlayer != null)
                    {
                        value = statcurrentWMaxFormatterAOfB.Format((int)GetCurrentStat(), (int)GetMaxStat());
                    }
                    return true;
                case "statwithmax":
                    value = "0";
                    if (localPlayer != null)
                    {
                        value = statcurrentWMaxFormatterAOfB.Format((int)GetCurrentStat(), (int)GetMaxStat());
                    }
                    return true;
                case "statmodifiedmax":
                    value = "0";
                    if (localPlayer != null)
                    {
                        value = statmodifiedmaxFormatter.Format(GetModifiedMax(), GetMaxStat());
                    }
                    return true;
                case "statuipercentage":
                    value = "0";
                    if (localPlayer != null)
                    {
                        value = statcurrentFormatterFloat.Format(GetStatUIPercentage());
                    }
                    return true;
                case "statpercentage":
                    value = "0";
                    if(localPlayer != null)
                    {
                        value = statcurrentFormatterInt.Format((int)(GetStatUIPercentage() * 100));
                    }
                    return true;
                case "statvisible":
                    value = IsStatVisible().ToString();
                    return true;
                default:
                    return base.GetBindingValue(ref value, bindingName);
            }
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
            float value = GetCurrentStat();

            bool result = cachedValue != value;
            cachedValue = value;

            return result;
        }

        protected virtual bool IsStatVisible()
        {
            if (localPlayer == null || localPlayer.IsDead())
            {
                return false;
            }

            return true;
        }

        protected abstract string GetStatName();

        protected abstract float GetStatUIPercentage();

        protected abstract float GetCurrentStat();

        protected abstract float GetMaxStat();

        protected abstract float GetModifiedMax();

    }
}

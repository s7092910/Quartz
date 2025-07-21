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

using UnityEngine;

namespace Quartz.Views
{
    public class XUiV_ScrollBar : XUiView
    {
        private const string TAG = "ScrollBar";

        protected UIScrollBar uiScrollBar;
        protected UIPanel panel;

        private Collider foregroundCollider;
        private Collider backgroundCollider;

        protected XUiView foregroundView;
        protected XUiView backgroundView;

        private string foregroundViewId;
        private string backgroundViewId;

        public bool HasXMLChildren { get => !string.IsNullOrEmpty(foregroundViewId) || !string.IsNullOrEmpty(backgroundViewId); }

        private string backgroundSpriteName = XUi.BlankTexture;
        private Color backgroundSpriteColor = Color.white;

        private bool hasBackgroundSprite { get => !backgroundSpriteName.Equals(XUi.BlankTexture) || !string.IsNullOrEmpty(backgroundViewId); }

        private string foregroundSpriteName;
        private Color foregroundSpriteColor = Color.white;

        private int foregroundPadding = 5;

        public UIScrollBar UiScrollBar
        {
            get { return uiScrollBar; }
            set
            {
                if (uiScrollBar != value)
                {
                    uiScrollBar = value;
                }
            }
        }

        public Color BackgroundSpriteColor
        {
            get
            {
                return backgroundSpriteColor;
            }
            set
            {
                if (backgroundSpriteColor.r != value.r || backgroundSpriteColor.g != value.g || backgroundSpriteColor.b != value.b || backgroundSpriteColor.a != value.a)
                {
                    backgroundSpriteColor = value;
                    isDirty = true;
                }
            }
        }

        public Color ForegroundSpriteColor
        {
            get
            {
                return foregroundSpriteColor;
            }
            set
            {
                if (foregroundSpriteColor.r != value.r || foregroundSpriteColor.g != value.g || foregroundSpriteColor.b != value.b || foregroundSpriteColor.a != value.a)
                {
                    foregroundSpriteColor = value;
                    isDirty = true;
                }
            }
        }

        public string BackgroundSpriteName
        {
            get
            {
                return backgroundSpriteName;
            }
            set
            {
                if (backgroundSpriteName != value)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        backgroundSpriteName = value;
                    }
                    else
                    {
                        backgroundSpriteName = XUi.BlankTexture;
                    }
                    isDirty = true;
                }
            }
        }

        public string ForegroundSpriteName
        {
            get
            {
                return foregroundSpriteName;
            }
            set
            {
                if (foregroundSpriteName != value)
                {
                    foregroundSpriteName = !string.IsNullOrEmpty(value) ? value : XUi.BlankTexture;
                    isDirty = true;
                }
            }
        }

        public XUiV_ScrollBar(string _id) : base(_id)
        {
        }

        public override void InitView()
        {
            if (!string.IsNullOrEmpty(foregroundViewId))
            {
                foregroundView = controller.GetChildById(foregroundViewId)?.ViewComponent;
            }

            if (!string.IsNullOrEmpty(backgroundViewId))
            {
                backgroundView = controller.GetChildById(backgroundViewId)?.ViewComponent;
            }

            if (foregroundView == null)
            {
                XUiV_Sprite foreground = new XUiC_Scrollbar_Sprite(id + "_foreground");
                foreground.xui = xui;
                foreground.Controller = new XUiController(controller);
                foreground.Controller.xui = xui;
                foreground.Controller.WindowGroup = controller.WindowGroup;

                foreground.SetDefaults(controller);
                foreground.UIAtlas = "UIAtlas";

                foreground.Position = new Vector2i(0, 0);
                foreground.SpriteName = foregroundSpriteName;
                foreground.Color = foregroundSpriteColor;
                foreground.ForegroundLayer = true;
                foreground.Pivot = pivot;
                foreground.Type = UIBasicSprite.Type.Sliced;
                foregroundView = foreground;
            }

            if (backgroundView == null)
            {
                XUiV_Sprite background = new XUiC_Scrollbar_Sprite(id + "_background");
                background.xui = xui;
                background.Controller = new XUiController(controller);
                background.Controller.xui = xui;
                background.Controller.WindowGroup = controller.WindowGroup;

                background.SetDefaults(controller);
                background.UIAtlas = "UIAtlas";

                background.Position = new Vector2i(0, 0);
                background.SpriteName = backgroundSpriteName;
                background.Color = backgroundSpriteColor;
                background.ForegroundLayer = true;
                background.Pivot = pivot;
                background.Type = UIBasicSprite.Type.Sliced;
                backgroundView = background;
            }

            base.InitView();
            uiScrollBar = uiTransform.GetComponent<UIScrollBar>();
            panel = uiTransform.GetComponent<UIPanel>();

            controller.xui.OnBuilt += OnBuild;

            UpdateData();
            initialized = true;
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
        }

        public override void UpdateData()
        {
            base.UpdateData();
            Logging.Out(TAG, id + ":UpdateData");

            if (isDirty)
            {
                Logging.Out(TAG, id + ":UpdateData Dirty");

                panel.depth = depth;

                if (foregroundCollider != null)
                {
                    foregroundCollider.enabled = true;
                }

                if (backgroundCollider != null)
                {
                    uiScrollBar.backgroundWidget.enabled = hasBackgroundSprite;
                    backgroundCollider.enabled = hasBackgroundSprite;
                }
            }

            if (!initialized)
            {
                uiTransform.localScale = Vector3.one;
                uiTransform.localPosition = new Vector3(position.x, position.y, 0f);
            }
        }

        public override bool ParseAttribute(string attribute, string value, XUiController parent)
        {
            if (attribute != null)
            {
                switch (attribute)
                {
                    case "foregroundcolor":
                        ForegroundSpriteColor = StringParsers.ParseColor32(value);
                        return true;
                    case "backgroundcolor":
                        BackgroundSpriteColor = StringParsers.ParseColor32(value);
                        return true;
                    case "foregroundsprite":
                        ForegroundSpriteName = value;
                        return true;
                    case "backgroundsprite":
                        BackgroundSpriteName = value;
                        return true;
                    case "foregroundname":
                        foregroundViewId = value;
                        return true;
                    case "backgroundname":
                        backgroundViewId = value;
                        return true;
                    case "padding":
                        foregroundPadding = int.Parse(value);
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, parent);
                }
            }
            return false;
        }


        public override void CreateComponents(GameObject _go)
        {
            _go.AddComponent<UIScrollBar>();
            _go.AddComponent<UIPanel>();
        }

        private void OnBuild()
        {

            uiScrollBar.setForegroundWidget(getSprite(foregroundView));
            uiScrollBar.setBackgroundWidget(getSprite(backgroundView));

            foregroundCollider = foregroundView.UiTransform.GetComponent<Collider>();
            backgroundCollider = backgroundView.UiTransform.GetComponent<Collider>();

            foregroundView.Size = new Vector2i(size.x - foregroundPadding, size.y - foregroundPadding);
            foregroundView.Depth = 2;

            backgroundView.Size = new Vector2i(size.x, size.y);
            backgroundView.Depth = 1;

            Logging.Out(TAG, "ForegroundView Type = " + foregroundView.GetType().Name);
            Logging.Out(TAG, "BackgroundView Type = " + backgroundView.GetType().Name);

        }

        private UISprite getSprite(XUiView view)
        {
            XUiV_Sprite xuiSprite = view as XUiV_Sprite;
            if (xuiSprite != null)
            {
                return xuiSprite.Sprite;
            }

            XUiV_Button xuiButton = view as XUiV_Button;
            if (xuiButton != null)
            {
                return xuiButton.Sprite;
            }

            return null;
        }
    }
}
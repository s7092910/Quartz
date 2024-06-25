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

using System.Collections.Generic;
using UnityEngine;

namespace Quartz.Views
{
    public class XUiV_ScrollView : XUiView
    {

        private const string TAG = "ScrollView";

        protected UIScrollView uiScrollView;
        protected UIScrollBar uiScrollBar;

        protected XUiV_ScrollViewContainer container;

        private bool opened;

        public UIScrollView UiScrollView
        {
            get { return uiScrollView; }
        }

        public UIScrollBar UiScrollBar
        {
            get { return uiScrollBar; }
            set
            {
                if (uiScrollBar != value)
                {
                    uiScrollBar = value;
                    isDirty = true;
                }
            }
        }

        public XUiV_ScrollViewContainer Container 
        { 
            get 
            { 
                return container; 
            } 

            internal set 
            { 
                container = value; 
            }
        }

        public UIDrawCall.Clipping Clipping
        {
            get { return clipping; }
            set
            {
                if (clipping != value)
                {
                    clipping = value;
                    isDirty = true;
                }
            }
        }

        public UIScrollView.Movement Movement
        {
            get { return movement; }
            set
            {
                if (movement != value)
                {
                    movement = value;
                    isDirty = true;
                }
            }
        }

        public Vector2 ClippingSoftness
        {
            get { return clippingSoftness; }
            set
            {
                if (clippingSoftness != value)
                {
                    clippingSoftness = value;
                    isDirty = true;
                }
            }
        }

        public UIScrollView.DragEffect DragEffect
        {
            get { return dragEffect; }
            set
            {
                if (dragEffect != value)
                {
                    dragEffect = value;
                    isDirty = true;
                }
            }
        }

        public float ScrollWheelFactor
        {
            get { return scrollWheelFactor; }
            set
            {
                if (scrollWheelFactor != value)
                {
                    scrollWheelFactor = value;
                    isDirty = true;
                }
            }
        }

        public bool ResetPositionOnOpen { get => resetPositionOnOpen; set => resetPositionOnOpen = value; }

        private UIScrollView.Movement movement;
        private UIScrollView.DragEffect dragEffect;
        private UIDrawCall.Clipping clipping = UIDrawCall.Clipping.SoftClip;

        private bool resetPositionOnOpen;

        private Vector2 clippingSize = new Vector2(-10000f, -10000f);
        private Vector2 clippingCenter = new Vector2(-10000f, -10000f);
        private Vector2 clippingSoftness;
        private float scrollWheelFactor;

        public XUiV_ScrollView(string _id) : base(_id)
        {
        }

        public override void InitView()
        {
            EventOnScroll = true;
            base.InitView();
            uiScrollView = uiTransform.GetComponent<UIScrollView>();
            UpdateData();
            controller.xui.OnBuilt += () =>
            {
                AddOnScrollListeners(controller);
                SetScrollbar();
            };
            initialized = true;
            collider.enabled = false;
        }

        public override void UpdateData()
        {

            Logging.Out(TAG, "UpdateData");
            if (isDirty)
            {
                Logging.Out(TAG, "UpdateData Dirty");
                uiScrollView.panel.depth = depth;
                uiScrollView.panel.softBorderPadding = true;
                uiScrollView.contentPivot = pivot;
                uiScrollView.dragEffect = dragEffect;

                if (clipping != UIDrawCall.Clipping.None)
                {
                    if (clippingCenter == new Vector2(-10000f, -10000f))
                    {
                        clippingCenter = new Vector2(size.x / 2, -size.y / 2);
                    }
                    if (clippingSize == new Vector2(-10000f, -10000f))
                    {
                        clippingSize = new Vector2(size.x, size.y);
                    }
                    UpdateClipping();
                }
            }

            if (!initialized)
            {
                uiTransform.localScale = Vector3.one;
                uiTransform.localPosition = new Vector3(position.x, position.y, 0f);

                uiScrollView.scrollWheelFactor = scrollWheelFactor;
                uiScrollView.movement = movement;
                uiScrollView.disableDragIfFits = true;
            }

            if (opened)
            {
                opened = false;
                ResetPosition();
            }
        }

        public override void CreateComponents(GameObject go)
        {
            go.AddComponent<UIPanel>();
            go.AddComponent<UIScrollView>();
        }

        public override void OnOpen()
        {
            base.OnOpen();
            opened = true;
        }

        public new void OnScroll(GameObject go, float delta)
        {
            uiScrollView.Scroll(delta);
            controller.Scrolled(delta);
            container.OnScrollViewScrolled(go, delta);
        }

        public void ResetPosition()
        {
            if (resetPositionOnOpen && (uiScrollView.shouldMoveVertically || uiScrollView.shouldMoveHorizontally))
            {
                uiScrollView.ResetPosition();
            }
        }

        public void ForceResetPosition()
        {
            uiScrollView.ResetPosition();
        }

        private void SetScrollbar()
        {
            if (uiScrollView == null || uiScrollBar == null)
            {
                return;
            }

            EventDelegate.Add(uiScrollBar.onChange, uiScrollView.OnScrollBar);
            uiScrollBar.BroadcastMessage("CacheDefaultColor", SendMessageOptions.DontRequireReceiver);

            if (movement == UIScrollView.Movement.Vertical)
            {
                uiScrollView.verticalScrollBar = uiScrollBar;
                uiScrollBar.alpha = uiScrollView.showScrollBars == UIScrollView.ShowCondition.Always || uiScrollView.shouldMoveVertically ? 1f : 0f;
                uiScrollView.horizontalScrollBar = null;
            }

            if (movement == UIScrollView.Movement.Horizontal)
            {
                uiScrollView.horizontalScrollBar = uiScrollBar;
                uiScrollBar.alpha = uiScrollView.showScrollBars == UIScrollView.ShowCondition.Always || uiScrollView.shouldMoveHorizontally ? 1f : 0f;
                uiScrollView.verticalScrollBar = null;
            }

            if (uiScrollBar.backgroundWidget != null) uiScrollBar.backgroundWidget.autoResizeBoxCollider = true;

            if (EventDelegate.IsValid(uiScrollBar.onChange))
            {
                EventDelegate.Execute(uiScrollBar.onChange);
            }
        }

        private void AddOnScrollListeners(XUiController controller)
        {
            List<XUiController> children = controller.Children;
            foreach (XUiController child in children)
            {
                XUiView childView = child.ViewComponent;
                if (childView != null && childView.HasEvent && !childView.EventOnScroll)
                {
                    UIEventListener uIEventListener = UIEventListener.Get(childView.UiTransform.gameObject);
                    uIEventListener.onScroll += OnScroll;
                }

                AddOnScrollListeners(child);
            }
        }

        private void UpdateClipping()
        {
            if (clipping != UIDrawCall.Clipping.None)
            {
                if (uiScrollView.panel.clipping != clipping)
                {
                    uiScrollView.panel.clipping = clipping;
                }
                if (uiScrollView.panel.clipSoftness != clippingSoftness)
                {
                    uiScrollView.panel.clipSoftness = clippingSoftness;
                }
                if (clippingSize.x < 0f)
                {
                    clippingSize.x = 0f;
                }
                if (clippingSize.y < 0f)
                {
                    clippingSize.y = 0f;
                }
                Vector4 vector = new Vector4(clippingCenter.x, clippingCenter.y, clippingSize.x, clippingSize.y);
                if (uiScrollView.panel.baseClipRegion != vector)
                {
                    uiScrollView.panel.baseClipRegion = vector;
                }
            }
        }
    }
}
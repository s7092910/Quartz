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
    public class ScrollViewContainer : global::XUiView
    {
        private const string TAG = "ScrollViewContainer";

        protected ScrollView scrollView;
        protected ScrollBarView scrollBar;
        protected XUiV_Grid grid;
        protected UIWidget widget;

        private UIScrollView.Movement scrollDirection = UIScrollView.Movement.Vertical;
        private Vector2 clippingSoftness = Vector2.zero;
        private UIDrawCall.Clipping clipping = UIDrawCall.Clipping.SoftClip;
        private string scrollbarId;
        private string scrollViewId;
        private bool resetPositionOnOpen = false;
        private bool overScroll = false;
        private float scrollWheelFactor = 2.5f;

        public ScrollView ScrollView { get { return scrollView; } }
        public ScrollBarView ScrollBar { get { return scrollBar; } }

        public ScrollViewContainer(string id) : base(id)
        {
        }

        public override void InitView()
        {
            scrollViewId = id;
            id = id + "Container";

            scrollView = new ScrollView(scrollViewId);
            scrollView.xui = xui;
            scrollView.Controller = new XUiController();
            scrollView.Controller.xui = xui;
            scrollView.SetDefaults(controller);
            scrollView.Controller.WindowGroup = controller.WindowGroup;
            scrollView.Container = this;

            SetScrollViewChildren();

            base.InitView();
            widget = uiTransform.GetComponent<UIWidget>();

            if (scrollbarId != null)
            {
                scrollBar = controller.Parent.GetChildById(scrollbarId)?.ViewComponent as ScrollBarView;
            }

            grid = FindGrid(controller);
            if (grid != null)
            {
                grid.OnSizeChanged += OnGridSizeChanged;
            }

            UpdateData();
            initialized = true;
        }

        protected override void CreateComponents(GameObject _go)
        {
            _go.AddComponent<UIWidget>();
        }

        public override void UpdateData()
        {
            base.UpdateData();
            Logging.Out(TAG, "UpdateData");

            if (isDirty)
            {
                Logging.Out(TAG, "UpdateData Dirty");
                if (collider != null)
                {
                    float x = size.x * 0.5f;
                    float num = size.y * 0.5f;
                    collider.center = new Vector3(x, -num, 0f);
                    collider.size = new Vector3(size.x * colliderScale, size.y * colliderScale, 0f);
                    collider.enabled = true;
                }

                scrollView.Position = Vector2i.zero;
                scrollView.Depth = depth + 1;
                scrollView.Pivot = pivot;
                scrollView.Size = size;
                scrollView.ClippingSoftness = clippingSoftness;
                scrollView.Clipping = clipping;
                scrollView.Movement = scrollDirection;
                scrollView.ResetPositionOnOpen = resetPositionOnOpen;
                scrollView.DragEffect = overScroll ? UIScrollView.DragEffect.MomentumAndSpring : UIScrollView.DragEffect.Momentum;
                scrollView.ScrollWheelFactor = scrollWheelFactor;

                uiTransform.localScale = Vector3.one;
            }

            if (!initialized)
            {
                if (scrollBar != null && scrollBar.UiScrollBar != null)
                {
                    scrollView.UiScrollBar = scrollBar.UiScrollBar;
                    scrollBar.UiScrollBar.fillDirection = scrollDirection == UIScrollView.Movement.Vertical ? UIProgressBar.FillDirection.TopToBottom : UIProgressBar.FillDirection.LeftToRight;
                }

                widget.depth = depth;
                widget.width = size.x;
                widget.height = size.y;

                UIEventListener uIEventListener = UIEventListener.Get(uiTransform.gameObject);
                uIEventListener.onScroll += scrollView.OnScroll;
            }
        }

        public override bool ParseAttribute(string attribute, string value, XUiController parent)
        {
            if (attribute != null)
            {
                switch (attribute)
                {
                    case "clippingsoftness":
                        clippingSoftness = StringParsers.ParseVector2(value);
                        isDirty = true;
                        return true;
                    case "clipping":
                        clipping = EnumUtils.Parse<UIDrawCall.Clipping>(value);
                        isDirty = true;
                        return true;
                    case "scrollbar":
                        scrollbarId = value;
                        return true;
                    case "scroll_direction":
                        scrollDirection = EnumUtils.Parse<UIScrollView.Movement>(value);
                        isDirty = true;
                        return true;
                    case "reset_position":
                        resetPositionOnOpen = StringParsers.ParseBool(value);
                        return true;
                    case "over_scroll":
                        overScroll = StringParsers.ParseBool(value);
                        isDirty = true;
                        return true;
                    case "scroll_speed":
                        float.TryParse(value, out scrollWheelFactor);
                        isDirty = true;
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, parent); ;
                }
            }
            return false;
        }

        internal void OnScrollViewScrolled(GameObject _go, float _delta)
        {
            controller.Scrolled(_delta);
        }

        private void SetScrollViewChildren()
        {
            List<XUiController> children = controller.Children;
            foreach (XUiController child in children)
            {
                child.Parent = scrollView.Controller;
                scrollView.Controller.AddChild(child);
            }
            children.Clear();

            scrollView.Controller.Parent = controller;
            controller.AddChild(scrollView.Controller);
        }

        private XUiV_Grid FindGrid(XUiController controller)
        {
            Queue<XUiController> q = new Queue<XUiController>();
            q.Enqueue(controller);
            XUiV_Grid grid = null;

            while (q.Count > 0)
            {
                XUiController curr = q.Dequeue();
                Logging.Out(TAG, "Find Grid curr view component = " + curr.ViewComponent);
                grid = curr.ViewComponent as XUiV_Grid;
                if (grid != null)
                {
                    return grid;
                }
                foreach (XUiController child in curr.Children)
                {
                    q.Enqueue(child);
                }
            }

            return grid;
        }

        private void OnGridSizeChanged(Vector2Int cells, Vector2 size)
        {
            scrollView.ForceResetPosition();
        }
    }
}
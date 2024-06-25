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

namespace QuartzOverhaul
{
    public class XUiBaseController : XUiController
    {

        public override sealed void Init()
        {
            base.Init();
            OnInit();
        }

        protected virtual void OnInit()
        {

        }

        public sealed override void Update(float dt)
        {
            base.Update(dt);
            OnUpdate(dt);
        }

        protected virtual void OnUpdate(float dt)
        {

        }

        public override sealed void OnDoubleClicked(int mouseButton)
        {
            base.OnDoubleClicked(mouseButton);
            DispatchOnClicked(this, mouseButton);
        }

        protected virtual bool OnDoubleClicked(XUiController reciever, int mouseButton)
        {
            return false;
        }

        public override sealed void OnDragged(EDragType dragType, Vector2 mousePositionDelta)
        {
            base.OnDragged(dragType, mousePositionDelta);
            DispatchOnDragged(this, dragType, mousePositionDelta);
        }

        protected virtual bool OnDragged(XUiController reciever, EDragType dragType, Vector2 mousePositionDelta)
        {
            return false;
        }

        public override sealed void OnHovered(bool hovered)
        {
            base.OnHovered(hovered);
            DispatchOnHovered(this, hovered);
        }

        protected virtual bool OnHovered(XUiController reciever, bool hovered)
        {
            return false;
        }

        public override sealed void OnPressed(int mouseButton)
        {
            base.OnPressed(mouseButton);
            DispatchOnPressed(this, mouseButton);
        }

        protected virtual bool OnPressed(XUiController reciever, int mouseButton)
        {
            return false;
        }

        public override sealed void OnScrolled(float delta)
        {
            base.OnScrolled(delta);
            DispatchOnScrolled(this, delta);
        }

        protected virtual bool OnScrolled(XUiController reciever, float delta)
        {
            return false;
        }

        public override sealed void OnSelected(bool selected)
        {
            base.OnSelected(selected);
            DispatchOnSelected(this, selected);
        }

        protected virtual bool OnSelected(XUiController reciever, bool selected)
        {
            return false;
        }

        private void DispatchOnClicked(XUiController reciever, int mouseButton)
        {
            bool consumed = OnDoubleClicked(reciever, mouseButton);

            if (consumed)
            {
                return;
            }

            XUiController parent = Parent;
            while (parent != null)
            {
                if (parent is XUiBaseController baseController)
                {
                    baseController.DispatchOnClicked(reciever, mouseButton);
                    break;
                }

                parent = parent.Parent;
            }
        }

        private void DispatchOnDragged(XUiController receiver, EDragType dragType, Vector2 mousePositionDelta)
        {
            bool consumed = OnDragged(receiver, dragType, mousePositionDelta);

            if (consumed)
            {
                return;
            }

            XUiController parent = Parent;
            while(parent != null)
            {
                if(parent is XUiBaseController baseController)
                {
                    baseController.DispatchOnDragged(receiver, dragType, mousePositionDelta);
                    break;
                }

                parent = parent.Parent;
            }
        }

        private void DispatchOnHovered(XUiController receiver, bool isHoveredOver)
        {
            bool consumed = OnHovered(receiver, isHoveredOver);

            if (consumed)
            {
                return;
            }

            XUiController parent = Parent;
            while (parent != null)
            {
                if (parent is XUiBaseController baseController)
                {
                    baseController.DispatchOnHovered(receiver, isHoveredOver);
                    break;
                }

                parent = parent.Parent;
            }
        }

        private void DispatchOnPressed(XUiController reciever, int mouseButton)
        {
            bool consumed = OnPressed(reciever, mouseButton);

            if (consumed)
            {
                return;
            }

            XUiController parent = Parent;
            while (parent != null)
            {
                if (parent is XUiBaseController baseController)
                {
                    baseController.DispatchOnPressed(reciever, mouseButton);
                    break;
                }

                parent = parent.Parent;
            }
        }

        private void DispatchOnScrolled(XUiController reciever, float delta)
        {
            bool consumed = OnScrolled(reciever, delta);

            if (consumed)
            {
                return;
            }

            XUiController parent = Parent;
            while (parent != null)
            {
                if (parent is XUiBaseController baseController)
                {
                    baseController.DispatchOnScrolled(reciever, delta);
                    break;
                }

                parent = parent.Parent;
            }
        }

        private void DispatchOnSelected(XUiController reciever, bool selected)
        {
            bool consumed = OnSelected(reciever, selected);

            if (consumed)
            {
                return;
            }

            XUiController parent = Parent;
            while (parent != null)
            {
                if (parent is XUiBaseController baseController)
                {
                    baseController.DispatchOnSelected(reciever, selected);
                    break;
                }

                parent = parent.Parent;
            }
        }

        public XUiController GetChildWithInterface<T>() where T : class
        {
            if (this is T)
            {
                return this;
            }

            foreach (XUiController child in children)
            {
                if (child is XUiBaseController baseChild)
                {
                    XUiController foundChild = baseChild.GetChildWithInterface<T>();
                    if (foundChild != null)
                    {
                        return foundChild;
                    }
                }
            }

            return null;
        }

        public XUiController[] GetChildrenWithInterface<T>(List<XUiController> list = null) where T : class
        {
            if (list == null)
            {
                list = new List<XUiController>();
            }

            if (this is T)
            {
                list.Add(this);
            }
            else
            {
                foreach (XUiController xuiController in children)
                {
                    if (xuiController is XUiBaseController baseChild)
                    {
                        baseChild.GetChildrenWithInterface<T>(list);
                    }
                }
            }

            return list.ToArray();
        }

        public XUiController GetParentWithInterface<T>() where T : class
        {
            if (this is T)
            {
                return this;
            }

            if (Parent != null && Parent is XUiBaseController parentBase)
            {
                return parentBase.GetParentWithInterface<T>();
            }

            return null;
        }

        public T GetChildByInterface<T>() where T : class
        {
            if (this is T foundChild)
            {
                return foundChild;
            }

            foreach (XUiController child in children)
            {
                if (child is XUiBaseController baseChild)
                {
                    foundChild = baseChild.GetChildByInterface<T>();
                    if (foundChild != null)
                    {
                        return foundChild;
                    }
                }
            }

            return null;
        }

        public T[] GetChildrenByInterface<T>(List<T> list = null) where T : class
        {
            if (list == null)
            {
                list = new List<T>();
            }

            if (this is T t)
            {
                list.Add(t);
            }
            else
            {
                foreach (XUiController xuiController in children)
                {
                    if (xuiController is XUiBaseController baseChild)
                    {
                        baseChild.GetChildrenByInterface<T>(list);
                    }
                }
            }

            return list.ToArray();
        }

        public T GetParentByInterface<T>() where T : class
        {
            if (this is T parent)
            {
                return parent;
            }

            if (Parent != null && Parent is XUiBaseController parentBase)
            {
                return parentBase.GetParentByInterface<T>();
            }

            return null;
        }

    }
}

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

namespace QuartzOverhaul
{
	public class SelectableEntry : XUiBaseController, IsSelectable
	{

		public static SelectableEntry currentSelectedEntry;

		private bool isSelected;

        public void Deselect()
        {
			if (currentSelectedEntry == this)
			{
				currentSelectedEntry.OnSelectedChanged(false);
				currentSelectedEntry.isSelected = false;
				currentSelectedEntry = null;
			}

			isSelected = false;

			OnSelectedChanged(false);
		}

        public void Select()
        {
			if (currentSelectedEntry != null)
			{
				currentSelectedEntry.Deselect();
			}

			isSelected = true;
			currentSelectedEntry = this;

			OnSelectedChanged(true);
		}
		public bool IsSelected()
		{
			return isSelected;
		}

		protected virtual void OnSelectedChanged(bool isSelected)
		{

		}
    }

	public interface IsSelectable
    {
		void Select();

		void Deselect();

		bool IsSelected();
    }
}

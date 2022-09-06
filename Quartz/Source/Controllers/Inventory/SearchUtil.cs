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

public static class SearchUtil
{
	private const string TAG = "SearchUtil";
    public static bool MatchesSearch(ItemStack item, string search)
    {
		ItemClass itemClass = item.itemValue.ItemClass;
		if (item.IsEmpty() || itemClass == null || string.IsNullOrEmpty(search))
        {
			return false;
        }

		string localizedName = itemClass.GetLocalizedItemName();
		if (localizedName == null)
		{
			localizedName = Localization.Get(itemClass.Name);
		}

		if (itemClass.Name.ContainsCaseInsensitive(search) || localizedName.ContainsCaseInsensitive(search) || item.itemValue.GetItemOrBlockId().ToString() == search.Trim())
		{
				return true;
		}
		else
		{
			string[] array = itemClass.Groups;
			if (itemClass.IsBlock())
			{
				if (item.itemValue.type < Block.list.Length)
				{ 
				array = Block.list[item.itemValue.type].GroupNames;
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != null && array[j].ContainsCaseInsensitive(search))
				{
					return true;
				}
			}
		}
		return false;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IB = ILPathways.Business;
using ILPathways.Common;

namespace IoerContentBusinessEntities
{
	public class GroupsManager
	{

		public static List<CodeItem> Codes_GroupType_Get()
		{
			CodeItem entity = new CodeItem();
			List<CodeItem> list = new List<CodeItem>();
			using (var context = new IsleContentContext())
			{
				List<Codes_GroupType> items = context.Codes_GroupType.Where( s => s.Id > 0).OrderBy(s => s.Title).ToList();

				if (items != null && items.Count > 0)
				{
					foreach (Codes_GroupType item in items)
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Title = item.Title;

						list.Add(entity);
					}
				}
			}
			return list;
		}

	}
}

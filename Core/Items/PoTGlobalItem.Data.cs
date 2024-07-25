using System.Collections.Generic;

namespace PathOfTerraria.Core.Items;

// Managed static (per-type) data.

partial class PoTGlobalItem
{
	private static Dictionary<int, PoTStaticItemData> _staticData = [];

	public override void Unload()
	{
		_staticData = null;
	}

	public static PoTStaticItemData GetStaticData(int type)
	{
		if (_staticData.TryGetValue(type, out PoTStaticItemData data))
		{
			return data;
		}

		return _staticData[type] = new PoTStaticItemData();
	}
}

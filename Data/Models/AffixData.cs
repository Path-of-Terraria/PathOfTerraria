using PathOfTerraria.Core;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PathOfTerraria.Data.Models;

public class ItemAffixData
{
	public class TierData
	{
		public float MinValue { get; set; }
		public float MaxValue { get; set; }
		public int MinimumLevel { get; set; }
	}

	public string AffixType { get; set; }
	public string EquipTypes { get; set; }
	public List<TierData> TierDatas { get; set; }

	public TierData GetAppropriateTierData(int level)
	{
		TierData data = null;

		foreach (TierData tier in TierDatas)
		{
			if (tier.MinimumLevel < level && (data is null || data.MinimumLevel < tier.MinimumLevel))
			{
				data = tier;
			}
		}

		return data;
	}

	public ItemType GetEquipTypes()
	{
		string[] split = EquipTypes.Split(' ');
		ItemType types = ItemType.None;

		foreach (string item in split)
		{
			if (Enum.TryParse(item, out ItemType type))
			{
				types |= type;
			}
			else
			{
				Console.WriteLine($"Affix attempted to load nonexisting {item} ItemType enumeration.");
			}
		}

		return types;
	}
}

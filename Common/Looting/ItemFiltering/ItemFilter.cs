using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Looting.ItemFiltering;

/// <summary>
///		A single named filter rule set. An item matches when every populated condition matches; an empty
///		<see cref="Rarities"/> set or <see cref="ItemType.None"/> base type means "match any".
/// </summary>
public sealed class ItemFilter
{
	public string Name { get; set; } = "New Filter";

	public int? MinItemLevel { get; set; }
	public int? MaxItemLevel { get; set; }

	public HashSet<ItemRarity> Rarities { get; } = [];
	public ItemType BaseTypes { get; set; } = ItemType.None;

	public bool Matches(Item item)
	{
		if (item is null || item.IsAir)
		{
			return false;
		}

		PoTInstanceItemData data = item.GetInstanceData();

		if (data is null)
		{
			return false;
		}

		if (MinItemLevel is { } min && data.RealLevel < min)
		{
			return false;
		}

		if (MaxItemLevel is { } max && data.RealLevel > max)
		{
			return false;
		}

		if (Rarities.Count > 0 && !Rarities.Contains(data.Rarity))
		{
			return false;
		}

		if (BaseTypes != ItemType.None && (BaseTypes & data.ItemType) == 0)
		{
			return false;
		}

		return true;
	}

	public ItemFilter Clone()
	{
		var copy = new ItemFilter
		{
			Name = Name,
			MinItemLevel = MinItemLevel,
			MaxItemLevel = MaxItemLevel,
			BaseTypes = BaseTypes
		};

		foreach (ItemRarity rarity in Rarities)
		{
			copy.Rarities.Add(rarity);
		}

		return copy;
	}

	public TagCompound Save()
	{
		var tag = new TagCompound
		{
			["name"] = Name,
			["baseTypes"] = (long)BaseTypes,
			["rarities"] = (int[])[.. Rarities.Select(r => (int)r)]
		};

		if (MinItemLevel is { } min)
		{
			tag["minLevel"] = min;
		}

		if (MaxItemLevel is { } max)
		{
			tag["maxLevel"] = max;
		}

		return tag;
	}

	public static ItemFilter Load(TagCompound tag)
	{
		var filter = new ItemFilter
		{
			Name = tag.GetString("name"),
			BaseTypes = (ItemType)tag.GetLong("baseTypes")
		};

		if (tag.ContainsKey("minLevel"))
		{
			filter.MinItemLevel = tag.GetInt("minLevel");
		}

		if (tag.ContainsKey("maxLevel"))
		{
			filter.MaxItemLevel = tag.GetInt("maxLevel");
		}

		if (tag.ContainsKey("rarities"))
		{
			foreach (int rarity in tag.GetIntArray("rarities"))
			{
				filter.Rarities.Add((ItemRarity)rarity);
			}
		}

		return filter;
	}

	public void NetSend(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write((long)BaseTypes);
		writer.Write(MinItemLevel ?? int.MinValue);
		writer.Write(MaxItemLevel ?? int.MaxValue);
		writer.Write((byte)Rarities.Count);

		foreach (ItemRarity rarity in Rarities)
		{
			writer.Write((sbyte)rarity);
		}
	}

	public static ItemFilter NetReceive(BinaryReader reader)
	{
		var filter = new ItemFilter
		{
			Name = reader.ReadString(),
			BaseTypes = (ItemType)reader.ReadInt64()
		};

		int min = reader.ReadInt32();
		int max = reader.ReadInt32();
		filter.MinItemLevel = min == int.MinValue ? null : min;
		filter.MaxItemLevel = max == int.MaxValue ? null : max;

		int rarityCount = reader.ReadByte();

		for (int i = 0; i < rarityCount; i++)
		{
			filter.Rarities.Add((ItemRarity)reader.ReadSByte());
		}

		return filter;
	}
}

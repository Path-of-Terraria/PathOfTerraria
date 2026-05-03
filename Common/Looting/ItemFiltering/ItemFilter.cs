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
		return Evaluate(item, out _);
	}

	/// <summary>
	///		Like <see cref="Matches"/>, but on rejection also returns the human-readable reason for the
	///		first condition that failed. Useful for surfacing why a specific item was filtered out.
	/// </summary>
	public bool Evaluate(Item item, out string reason)
	{
		reason = null;

		if (item is null || item.IsAir)
		{
			reason = "item is null/air";
			return false;
		}

		PoTInstanceItemData data = item.GetInstanceData();

		if (data is null)
		{
			reason = "no PoT data";
			return false;
		}

		if (MinItemLevel is { } min && data.RealLevel < min)
		{
			reason = $"Level {data.RealLevel} < Min {min}";
			return false;
		}

		if (MaxItemLevel is { } max && data.RealLevel > max)
		{
			reason = $"Level {data.RealLevel} > Max {max}";
			return false;
		}

		if (Rarities.Count > 0 && !Rarities.Contains(data.Rarity))
		{
			reason = $"Rarity {data.Rarity} not in {{{string.Join(", ", Rarities)}}}";
			return false;
		}

		if (BaseTypes != ItemType.None && (BaseTypes & data.ItemType) == 0)
		{
			reason = $"BaseType {data.ItemType} not in {BaseTypes}";
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

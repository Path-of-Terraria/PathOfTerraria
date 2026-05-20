using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Looting.ItemFiltering;

public enum ItemFilterRuleAction : byte
{
	Show,
	Hide,
	Highlight
}

/// <summary>
///		A saved loot filter made of ordered rules. Rules are evaluated from first to last; when more
///		than one rule matches, the later rule wins. If no rule matches, the item is shown.
/// </summary>
public sealed class ItemFilter
{
	public string Name { get; set; } = "New Filter";

	public List<ItemFilterRule> Rules { get; } = [];

	public bool Matches(Item item)
	{
		return Evaluate(item, out ItemFilterEvaluationResult _);
	}

	public bool Evaluate(Item item, out LocalizedText reason)
	{
		bool isShown = Evaluate(item, out ItemFilterEvaluationResult result);
		reason = result.Reason;
		return isShown;
	}

	public bool Evaluate(Item item, out ItemFilterEvaluationResult result)
	{
		const string Path = "Mods.PathOfTerraria.UI.ItemFilter.Reason.";

		if (item is null || item.IsAir)
		{
			result = ItemFilterEvaluationResult.Hidden(null, Language.GetText(Path + "ItemMissing"));
			return false;
		}

		ItemFilterRule matchedRule = null;

		foreach (ItemFilterRule rule in Rules)
		{
			if (rule.Matches(item))
			{
				matchedRule = rule;
			}
		}

		if (matchedRule is null)
		{
			result = ItemFilterEvaluationResult.Shown(null, Language.GetText(Path + "NoMatchingRule"));
			return true;
		}

		string reasonKey = matchedRule.Action switch
		{
			ItemFilterRuleAction.Hide => "HiddenByRule",
			ItemFilterRuleAction.Highlight => "HighlightedByRule",
			_ => "ShownByRule"
		};

		LocalizedText reason = Language.GetText(Path + reasonKey).WithFormatArgs(matchedRule.Name);

		result = matchedRule.Action == ItemFilterRuleAction.Hide
			? ItemFilterEvaluationResult.Hidden(matchedRule, reason)
			: ItemFilterEvaluationResult.Shown(matchedRule, reason);

		return result.IsShown;
	}

	public ItemFilter Clone()
	{
		var copy = new ItemFilter
		{
			Name = Name
		};

		foreach (ItemFilterRule rule in Rules)
		{
			copy.Rules.Add(rule.Clone());
		}

		return copy;
	}

	public TagCompound Save()
	{
		return new TagCompound
		{
			["name"] = Name,
			["rules"] = Rules.Select(r => r.Save()).ToList()
		};
	}

	public static ItemFilter Load(TagCompound tag)
	{
		var filter = new ItemFilter
		{
			Name = tag.GetString("name")
		};

		if (tag.GetList<TagCompound>("rules") is { } rules)
		{
			foreach (TagCompound rule in rules)
			{
				filter.Rules.Add(ItemFilterRule.Load(rule));
			}
		}

		return filter;
	}

	public void NetSend(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write(Rules.Count);

		foreach (ItemFilterRule rule in Rules)
		{
			rule.NetSend(writer);
		}
	}

	public static ItemFilter NetReceive(BinaryReader reader)
	{
		var filter = new ItemFilter
		{
			Name = reader.ReadString()
		};

		int ruleCount = reader.ReadInt32();

		for (int i = 0; i < ruleCount; i++)
		{
			filter.Rules.Add(ItemFilterRule.NetReceive(reader));
		}

		return filter;
	}
}

public sealed class ItemFilterRule
{
	public string Name { get; set; } = "New Rule";
	public ItemFilterRuleAction Action { get; set; } = ItemFilterRuleAction.Show;

	public int? MinItemLevel { get; set; }
	public int? MaxItemLevel { get; set; }

	public HashSet<ItemRarity> Rarities { get; } = [];
	public ItemType BaseTypes { get; set; } = ItemType.None;

	public ItemFilterRulePresentation Presentation { get; } = new();

	public bool RequiresPoTData =>
		MinItemLevel is not null || MaxItemLevel is not null || Rarities.Count > 0 || BaseTypes != ItemType.None;

	public bool Matches(Item item)
	{
		return EvaluateConditions(item, out _);
	}

	/// <summary>
	///		Returns whether the item's data matches this rule's conditions. Empty conditions match any
	///		non-air item, which is what enables broad rules like "hide all".
	/// </summary>
	public bool EvaluateConditions(Item item, out LocalizedText reason)
	{
		const string Path = "Mods.PathOfTerraria.UI.ItemFilter.Reason.";

		reason = null;

		if (item is null || item.IsAir)
		{
			reason = Language.GetText(Path + "ItemMissing");
			return false;
		}

		if (!RequiresPoTData)
		{
			return true;
		}

		PoTInstanceItemData data = item.GetInstanceData();

		if (data is null)
		{
			reason = Language.GetText(Path + "MissingData");
			return false;
		}

		if (MinItemLevel is { } min && data.RealLevel < min)
		{
			reason = Language.GetText(Path + "LevelBelowMin").WithFormatArgs(data.RealLevel, min);
			return false;
		}

		if (MaxItemLevel is { } max && data.RealLevel > max)
		{
			reason = Language.GetText(Path + "LevelAboveMax").WithFormatArgs(data.RealLevel, max);
			return false;
		}

		if (Rarities.Count > 0 && !Rarities.Contains(data.Rarity))
		{
			reason = Language.GetText(Path + "RarityNotAllowed").WithFormatArgs(data.Rarity, string.Join(", ", Rarities));
			return false;
		}

		if (BaseTypes != ItemType.None && (BaseTypes & data.ItemType) == 0)
		{
			reason = Language.GetText(Path + "BaseTypeNotAllowed").WithFormatArgs(data.ItemType, BaseTypes);
			return false;
		}

		return true;
	}

	public ItemFilterRule Clone()
	{
		var copy = new ItemFilterRule
		{
			Name = Name,
			Action = Action,
			MinItemLevel = MinItemLevel,
			MaxItemLevel = MaxItemLevel,
			BaseTypes = BaseTypes
		};

		foreach (ItemRarity rarity in Rarities)
		{
			copy.Rarities.Add(rarity);
		}

		copy.Presentation.CopyFrom(Presentation);
		return copy;
	}

	public TagCompound Save()
	{
		var tag = new TagCompound
		{
			["name"] = Name,
			["action"] = (byte)Action,
			["baseTypes"] = (long)BaseTypes,
			["rarities"] = (int[])[.. Rarities.Select(r => (int)r)],
			["presentation"] = Presentation.Save()
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

	public static ItemFilterRule Load(TagCompound tag)
	{
		var rule = new ItemFilterRule
		{
			Name = tag.GetString("name"),
			Action = tag.TryGet("action", out byte action) ? (ItemFilterRuleAction)action : ItemFilterRuleAction.Show,
			BaseTypes = (ItemType)tag.GetLong("baseTypes")
		};

		if (tag.ContainsKey("minLevel"))
		{
			rule.MinItemLevel = tag.GetInt("minLevel");
		}

		if (tag.ContainsKey("maxLevel"))
		{
			rule.MaxItemLevel = tag.GetInt("maxLevel");
		}

		if (tag.ContainsKey("rarities"))
		{
			foreach (int rarity in tag.GetIntArray("rarities"))
			{
				rule.Rarities.Add((ItemRarity)rarity);
			}
		}

		if (tag.TryGet("presentation", out TagCompound presentation))
		{
			rule.Presentation.CopyFrom(ItemFilterRulePresentation.Load(presentation));
		}

		return rule;
	}

	public void NetSend(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write((byte)Action);
		writer.Write((long)BaseTypes);
		writer.Write(MinItemLevel ?? int.MinValue);
		writer.Write(MaxItemLevel ?? int.MaxValue);
		writer.Write((byte)Rarities.Count);

		foreach (ItemRarity rarity in Rarities)
		{
			writer.Write((sbyte)rarity);
		}

		Presentation.NetSend(writer);
	}

	public static ItemFilterRule NetReceive(BinaryReader reader)
	{
		var rule = new ItemFilterRule
		{
			Name = reader.ReadString(),
			Action = (ItemFilterRuleAction)reader.ReadByte(),
			BaseTypes = (ItemType)reader.ReadInt64()
		};

		int min = reader.ReadInt32();
		int max = reader.ReadInt32();
		rule.MinItemLevel = min == int.MinValue ? null : min;
		rule.MaxItemLevel = max == int.MaxValue ? null : max;

		int rarityCount = reader.ReadByte();

		for (int i = 0; i < rarityCount; i++)
		{
			rule.Rarities.Add((ItemRarity)reader.ReadSByte());
		}

		rule.Presentation.CopyFrom(ItemFilterRulePresentation.NetReceive(reader));
		return rule;
	}
}

/// <summary>
///		Reserved per-rule presentation data. The filter evaluation returns the matched rule so future
///		loot beams, label colors, and audio cues can be applied without changing rule matching again.
/// </summary>
public sealed class ItemFilterRulePresentation
{
	public void CopyFrom(ItemFilterRulePresentation other)
	{
	}

	public TagCompound Save()
	{
		return new TagCompound();
	}

	public static ItemFilterRulePresentation Load(TagCompound tag)
	{
		return new ItemFilterRulePresentation();
	}

	public void NetSend(BinaryWriter writer)
	{
	}

	public static ItemFilterRulePresentation NetReceive(BinaryReader reader)
	{
		return new ItemFilterRulePresentation();
	}
}

public readonly struct ItemFilterEvaluationResult
{
	public bool IsShown { get; }
	public bool IsHighlighted => IsShown && Rule?.Action == ItemFilterRuleAction.Highlight;
	public ItemFilterRule Rule { get; }
	public LocalizedText Reason { get; }

	private ItemFilterEvaluationResult(bool isShown, ItemFilterRule rule, LocalizedText reason)
	{
		IsShown = isShown;
		Rule = rule;
		Reason = reason;
	}

	public static ItemFilterEvaluationResult Shown(ItemFilterRule rule, LocalizedText reason)
	{
		return new ItemFilterEvaluationResult(true, rule, reason);
	}

	public static ItemFilterEvaluationResult Hidden(ItemFilterRule rule, LocalizedText reason)
	{
		return new ItemFilterEvaluationResult(false, rule, reason);
	}
}

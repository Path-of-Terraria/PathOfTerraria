using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes;

#nullable enable

public abstract class ItemAffix : Affix
{
	public virtual void ApplyAffix(Player player, EntityModifier modifier, Item item) { }

	/// <summary>
	/// Adds all of this affix's tooltip lines to the provided handler.
	/// <br/> Most often you will want to overload <see cref="CreateDefaultTooltip"/> instead.
	/// </summary>
	public virtual void ApplyTooltips(Player player, Item item, AffixTooltips handler)
	{
		PoTInstanceItemData instanceData = item.GetInstanceData();
		handler.AddOrModify(GetType(), CreateDefaultTooltip(player, instanceData.ItemType, instanceData.RealLevel));
	}
	/// <inheritdoc cref="ApplyTooltips(Player, Item, AffixTooltips)"/>
	public virtual void ApplyTooltips(Player player, ItemType itemType, int itemLevel, AffixTooltips handler)
	{
		handler.AddOrModify(GetType(), CreateDefaultTooltip(player, itemType, itemLevel));
	}

	protected virtual AffixTooltipLine CreateDefaultTooltip(Player player, ItemType itemType, int itemLevel)
	{
		ItemAffixData? data = TryGetData(itemType);

		if (data is null) // Data can be null if the affix doesn't exist in the json data. This skips the checks below.
		{
			return new AffixTooltipLine
			{
				Text = this.GetLocalization("Description"),
				Value = Value,
				Tier = null,
				ValueRollRange = null,
				Corrupt = IsCorruptedAffix,
				Implicit = IsImplicit,
			};
		}

		// PoTInstanceItemData itemData = item.GetInstanceData();
		ItemAffixData.TierData tierData = data.Tiers[Tier];
		
		(int tierMin, int tierMax) = data.GetPossibleTierRange(itemLevel);

		return new AffixTooltipLine
		{
			Text = this.GetLocalization("Description"),
			Value = Value,
			Tier = (Tier, tierMin, tierMax),
			ValueRollRange = (tierData.MinValue, tierData.MaxValue),
			Corrupt = IsCorruptedAffix,
			Implicit = IsImplicit,
		};
	}
	
	protected virtual AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		int level = GetItemLevel.Invoke(item);
		return CreateDefaultTooltip(player, item);
	}

	/// <inheritdoc cref="AffixRegistry.GetItemData(Type)"/>
	public IEnumerable<ItemAffixData> GetData() { return AffixRegistry.GetItemData(GetType()); }
	/// <inheritdoc cref="AffixRegistry.GetItemData(Type, Item)"/>
	public ItemAffixData GetData(Item item) { return AffixRegistry.GetItemData(GetType(), item); }
	/// <inheritdoc cref="AffixRegistry.GetItemData(Type, ItemType)"/>
	public ItemAffixData GetData(ItemType exactItemType) { return AffixRegistry.GetItemData(GetType(), exactItemType); }

	/// <inheritdoc cref="AffixRegistry.TryGetItemData(Type)"/>
	public IEnumerable<ItemAffixData>? TryGetData() { return AffixRegistry.TryGetItemData(GetType()); }
	/// <inheritdoc cref="AffixRegistry.TryGetItemData(Type, Item)"/>
	public ItemAffixData? TryGetData(Item item) { return AffixRegistry.TryGetItemData(GetType(), item); }
	/// <inheritdoc cref="AffixRegistry.TryGetItemData(Type, ItemType)"/>
	public ItemAffixData? TryGetData(ItemType exactItemType) { return AffixRegistry.TryGetItemData(GetType(), exactItemType); }

	public Influence GetRequiredInfluence(Item item)
	{
		const Influence Default = Influence.None;
		return TryGetData(item)?.GetInfluences() ?? Default;
	}
	public ItemType GetPossibleTypes()
	{
		const ItemType Default = ItemType.None;
		return TryGetData()?.Aggregate(Default, (current, data) => current | data.GetEquipTypes()) ?? Default;
	}

	internal override void CreateLocalization()
	{
		Language.GetOrRegister(this.GetLocalizationKey("Description"), () => "{1}{0} to stat");
	}
}
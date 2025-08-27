using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes;

public abstract class ItemAffix : Affix
{
	public Influence RequiredInfluence => GetData().GetInfluences();
	public ItemType PossibleTypes => GetData().GetEquipTypes();

	public virtual void ApplyAffix(Player player, EntityModifier modifier, Item item) { }

	/// <summary>
	/// Adds all of this affix's tooltip lines to the provided handler.
	/// <br/> Most often you will want to overload <see cref="CreateDefaultTooltip"/> instead.
	/// </summary>
	public virtual void ApplyTooltips(Player player, Item item, AffixTooltips handler)
	{
		handler.AddOrModify(GetType(), CreateDefaultTooltip(player, item));
	}

	protected virtual AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		ItemAffixData data = GetData();
		ItemAffixData.TierData tierData = data.Tiers[Tier];

		int level = GetItemLevel.Invoke(item);
		(int tierMin, int tierMax) = data.GetPossibleTierRange(level);

		return new AffixTooltipLine
		{
			Text = this.GetLocalization("Description"),
			Value = Value,
			Tier = (Tier, tierMin, tierMax),
			ValueRollRange = (tierData.MinValue, tierData.MaxValue),
			Corrupt = IsCorruptedAffix,
		};
	}

	/// <summary>
	/// Retrieves the affix data for the current <see cref="ItemAffix"/> instance.
	/// </summary>
	/// <returns></returns>
	public ItemAffixData GetData()
	{
		return AffixRegistry.TryGetAffixData(GetType());
	}

	internal override void CreateLocalization()
	{
		Language.GetOrRegister(this.GetLocalizationKey("Description"), () => "{1}{0} to stat");
	}
}
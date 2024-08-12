using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes;

public abstract class ItemAffix : Affix
{
	public Influence RequiredInfluence => GetData().GetInfluences();
	public ItemType PossibleTypes => GetData().GetEquipTypes();

	public virtual void ApplyAffix(Player player, EntityModifier modifier, Item item) { }

	public virtual void ApplyTooltip(Player player, Item item, AffixTooltipsHandler handler)
	{
		handler.AddOrModify(GetType(), Value, Language.GetText($"Mods.PathOfTerraria.Affixes.{GetType().Name}.Description"), null);
	}

	/// <summary>
	/// Retrieves the affix data for the current <see cref="ItemAffix"/> instance.
	/// </summary>
	/// <returns></returns>
	public ItemAffixData GetData()
	{
		return AffixRegistry.TryGetAffixData(GetType());
	}

	public string GetTooltip(Item gear, Player player)
	{
		EntityModifier modifier = new();
		ApplyAffix(player, modifier, gear);

		string tooltip = "";

		List<string> affixes = EntityModifier.GetChangeOnlyStrings(modifier);

		if (affixes.Count != 0)
		{
			tooltip = affixes.First(); // idk if there will ever be more..?
		}

		return tooltip;
	}

	internal override void CreateLocalization()
	{
		Language.GetOrRegister($"Mods.PathOfTerraria.Affixes.{GetType().Name}.Description", () => "+{0} to stat");
	}
}
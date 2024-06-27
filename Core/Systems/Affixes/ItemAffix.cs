using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Data;
using PathOfTerraria.Data.Models;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Affixes;

internal abstract class ItemAffix : Affix
{
	// public virtual ModifierType ModifierType => ModifierType.Passive;
	// public virtual bool IsFlat => true; // alternative is percent
	// public virtual bool Round => false;
	public virtual Influence RequiredInfluence => Influence.None;

	public ItemType PossibleTypes => GetData().GetEquipTypes();

	public virtual void ApplyAffix(EntityModifier modifier, PoTItem gear) { }

	/// <summary>
	/// Retrieves the affix data for the current <see cref="ItemAffix"/> instance.
	/// </summary>
	/// <returns></returns>
	public ItemAffixData GetData()
	{
		return AffixRegistry.TryGetAffixData(GetType());
	}

	public string GetTooltip(PoTItem gear)
	{
		EntityModifier modifier = new();
		ApplyAffix(modifier, gear);

		string tooltip = "";

		List<string> affixes = EntityModifier.GetChangeOnlyStrings(modifier);

		if (affixes.Any())
		{
			tooltip = affixes.First(); // idk if there will ever be more..?
		}

		return tooltip;
	}
}
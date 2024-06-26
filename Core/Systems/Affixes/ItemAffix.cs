using PathOfTerraria.Content.Items.Gear;
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
	public abstract ItemType PossibleTypes { get; }

	public virtual void ApplyAffix(EntityModifier modifier, PoTItem gear) { }

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
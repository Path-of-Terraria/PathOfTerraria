using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems;
using System.Collections.Generic;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Core.Items;

// Manages affixes.
// NOTE: Some invocations of methods defined here lie elsewhere, where
// appropriate.
// NOTE: Affix rolling logic is in PoTGlobalItem.Rolling.cs.

partial class PoTGlobalItem
{
	#region Affix methods
	public static void ApplyAffixes(Item item, EntityModifier entityModifier)
	{
		foreach (ItemAffix affix in item.GetInstanceData().Affixes)
		{
			affix.ApplyAffix(entityModifier, item);
		}
	}

	public static void ClearAffixes(Item item)
	{
		item.GetInstanceData().Affixes.Clear();
	}

	// Removed ability to override; restore?
	public static int GetAffixCount(Item item)
	{
		return item.GetInstanceData().Rarity switch
		{
			Rarity.Magic => 2,
			Rarity.Rare => Main.rand.Next(3, 5),
			_ => 0
		};
	}
	#endregion

	public override void UpdateEquip(Item item, Player player)
	{
		base.UpdateEquip(item, player);

		ApplyAffixes(item, player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier);
	}
}

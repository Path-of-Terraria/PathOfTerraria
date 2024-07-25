using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems;
using System.Collections.Generic;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Core.Items;

// Manages affixes.
// NOTE: Some invocations of methods defined here lie elsewhere, where
// appropriate.

partial class PoTGlobalItem
{
	#region Affix methods
	public void ApplyAffixes(Item item, EntityModifier entityModifier)
	{
		foreach (ItemAffix affix in item.GetInstanceData().Affixes)
		{
			affix.ApplyAffix(entityModifier, item);
		}
	}

	public void ClearAffixes(Item item)
	{
		item.GetInstanceData().Affixes.Clear();
	}
	#endregion

	public override void UpdateEquip(Item item, Player player)
	{
		base.UpdateEquip(item, player);

		ApplyAffixes(item, player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier);
	}
}

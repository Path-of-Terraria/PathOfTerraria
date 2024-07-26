using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Core.Items;

// Manages affixes.

partial class PoTGlobalItem
{
	public override void UpdateEquip(Item item, Player player)
	{
		base.UpdateEquip(item, player);

		PoTItemHelper.ApplyAffixes(item, player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier);
	}
}

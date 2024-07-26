using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Core.Items;

internal sealed partial class PoTGlobalItem : GlobalItem
{
	// IMPORTANT: Called *after* ModItem::SetDefaults.
	// https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/ModLoader/Core/GlobalLoaderUtils.cs#L20
	public override void SetDefaults(Item entity)
	{
		base.SetDefaults(entity);

		PoTItemHelper.Roll(entity, PoTItemHelper.PickItemLevel());
	}

	public override void UpdateEquip(Item item, Player player)
	{
		base.UpdateEquip(item, player);

		PoTItemHelper.ApplyAffixes(item, player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier);
	}
}

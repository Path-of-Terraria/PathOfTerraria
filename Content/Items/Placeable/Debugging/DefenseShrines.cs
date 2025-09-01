#if DEBUG
using PathOfTerraria.Content.Tiles.Maps;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Debugging;

internal class ForestDefenseShrineItem : ModItem
{
	protected virtual int Style => 0;

	public override void SetStaticDefaults()
	{
		ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<DefenseShrine>(), Style);
		Item.width = 16;
		Item.height = 16;
	}
}

internal class DesertDefenseShrineItem : ForestDefenseShrineItem
{
	protected override int Style => 1;
}
#endif
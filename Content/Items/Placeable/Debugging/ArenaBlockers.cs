#if DEBUG
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Content.Tiles.Maps.Forest;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Debugging;

internal class ArenaBlockerItem : ModItem
{
	public override void SetStaticDefaults()
	{
		ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<ArenaBlocker>());
		Item.width = 16;
		Item.height = 16;
	}
}

internal class HiveBlockerItem : ModItem
{
	public override void SetStaticDefaults()
	{
		ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<HiveBlocker>());
		Item.width = 16;
		Item.height = 16;
	}
}

internal class LivingWoodBlockerItem : ModItem
{
	public override void SetStaticDefaults()
	{
		ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<LivingWoodBlocker>());
		Item.width = 16;
		Item.height = 16;
	}
}
#endif
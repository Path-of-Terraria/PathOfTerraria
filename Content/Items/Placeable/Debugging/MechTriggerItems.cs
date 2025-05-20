#if DEBUG
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Debugging;

internal class DroneTriggerItem : ModItem
{
	protected virtual int Style => 0;

	public override bool IsLoadingEnabled(Mod mod)
	{
		return PoTMod.CheatModEnabled;
	}

	public override void SetStaticDefaults()
	{
		ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<MechSpawnerTile>());
		Item.width = 16;
		Item.height = 16;
		Item.placeStyle = Style;
	}

	public override void HoldItem(Player player)
	{
		player.InfoAccMechShowWires = true;
	}
}

internal class CircuitSkullTriggerItem : DroneTriggerItem
{
	protected override int Style => 1;
}

internal class SecurityDroneTriggerItem : DroneTriggerItem
{
	protected override int Style => 2;
}

internal class EnergizerCoreTriggerItem : DroneTriggerItem
{
	protected override int Style => 3;
}
#endif
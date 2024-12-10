#if DEBUG
using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Debugging;

internal class LavaSlimeTriggerItem : ModItem
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
		Item.DefaultToPlaceableTile(ModContent.TileType<UnderworldSpawnerTile>());
		Item.width = 16;
		Item.height = 16;
		Item.placeStyle = Style;
	}

	public override void HoldItem(Player player)
	{
		player.InfoAccMechShowWires = true;
	}
}

internal class DemonTriggerItem : LavaSlimeTriggerItem
{
	protected override int Style => 1;
}

internal class ImpTriggerItem : LavaSlimeTriggerItem
{
	protected override int Style => 2;
}

internal class LavaBatTriggerItem : LavaSlimeTriggerItem
{
	protected override int Style => 3;
}

internal class DevilTriggerItem : LavaSlimeTriggerItem
{
	protected override int Style => 4;
}

internal class FireMawTriggerItem : LavaSlimeTriggerItem
{
	protected override int Style => 5;
}
#endif
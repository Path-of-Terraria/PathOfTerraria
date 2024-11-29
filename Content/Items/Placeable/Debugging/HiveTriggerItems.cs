#if DEBUG
using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Debugging;

internal class HornetTriggerItem : ModItem
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
		Item.DefaultToPlaceableTile(ModContent.TileType<HiveSpawner>());
		Item.width = 16;
		Item.height = 16;
		Item.placeStyle = Style;
	}

	public override void HoldItem(Player player)
	{
		player.InfoAccMechShowWires = true;
	}
}

internal class BeeTriggerItem : HornetTriggerItem
{
	protected override int Style => 1;
}

internal class MossHornetTriggerItem : HornetTriggerItem
{
	protected override int Style => 2;
}
#endif
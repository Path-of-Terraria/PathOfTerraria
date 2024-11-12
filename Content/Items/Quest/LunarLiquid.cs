using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class LunarLiquid : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.rare = ItemRarityID.Quest;
		Item.Size = new Vector2(24, 18);
		Item.consumable = false;
		Item.buffTime = 25;
		Item.buffType = BuffID.VortexDebuff;
		Item.useTime = Item.useAnimation = 30;
		Item.useStyle = ItemUseStyleID.DrinkLiquid;
	}

	public override bool? UseItem(Player player)
	{
		Main.NewText(Main.MouseWorld.ToTileCoordinates());
		return true;
	}
}

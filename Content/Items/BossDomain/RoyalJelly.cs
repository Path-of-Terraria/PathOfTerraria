using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

internal class RoyalJelly : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Abeemination);
		Item.Size = new Vector2(48, 36);

		Item.autoReuse = true;
		Item.useTime = 2;
		Item.useAnimation = 2;
		Item.useStyle = 1;
	}

	public override bool? UseItem(Player player)
	{
		Point16 pos = Main.MouseWorld.ToTileCoordinates16();
		int checkType = TileID.JunglePlants;
		int style = 8;// WorldGen.genRand.NextBool() && checkType == TileID.JunglePlants ? 8 : WorldGen.genRand.Next(24);
		WorldGen.PlaceTile(pos.X, pos.Y, checkType, true, false, style: style);
		Tile tile = Main.tile[pos];
		tile.TileFrameX = 8 * 18;
		return true;
		NPC.SpawnOnPlayer(player.whoAmI, NPCID.QueenBee);
		return true;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<RoyalJellyClump>(2)
			.Register();
	}
}

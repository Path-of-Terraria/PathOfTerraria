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
		int style3x2 = WorldGen.genRand.NextBool() ? WorldGen.genRand.Next(3) : WorldGen.genRand.Next(6) + 6;

		WorldGen.PlaceJunglePlant(pos.X, pos.Y, TileID.PlantDetritus, Main.rand.Next(12), 1);
		Main.NewText(Main.tile[pos].TileFrameX + " " + Main.tile[pos].TileFrameY);
		WorldGen.SquareTileFrame(pos.X ,pos.Y);
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

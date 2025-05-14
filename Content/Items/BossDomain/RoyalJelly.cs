using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

internal class RoyalJelly : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Abeemination);
		Item.Size = new Vector2(48, 36);
	}

	public override bool? UseItem(Player player)
	{
		Point16 pos = Main.MouseWorld.ToTileCoordinates16();
		DestroyerDomain.MakeTower(pos.X, pos.Y);

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

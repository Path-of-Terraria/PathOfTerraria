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

using PathOfTerraria.Content.Tiles.Furniture;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable;

public class BrokenBookcaseItem : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 3;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<BrokenBookcase>());
		Item.width = 24;
		Item.height = 32;
		Item.value = 0;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Book, 6)
			.AddIngredient(ItemID.Wood, 18)
			.AddCondition(Condition.InGraveyard)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
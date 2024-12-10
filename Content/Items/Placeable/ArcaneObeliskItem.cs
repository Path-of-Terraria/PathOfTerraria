using PathOfTerraria.Content.Tiles;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable;

public class ArcaneObeliskItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<ArcaneObeliskTile>());

		Item.width = 24;
		Item.height = 30;
	}

	public override bool CanUseItem(Player player)
	{
		return ModContent.GetInstance<ArcaneObeliskTile.ArcaneObeliskSystem>().ArcaneObeliskLocation is null;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Diamond)
			.AddIngredient(ItemID.Emerald)
			.AddIngredient(ItemID.Ruby)
			.AddRecipeGroup(RecipeGroupID.IronBar)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
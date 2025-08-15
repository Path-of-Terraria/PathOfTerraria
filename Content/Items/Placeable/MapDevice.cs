using PathOfTerraria.Content.Tiles.Furniture;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable;

public class MapDevice : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<MapDevicePlaceable>());
		Item.rare = ItemRarityID.Blue;
		Item.width = 36;
		Item.height = 18;
		Item.value = 0;
	}
	
	public override void AddRecipes()
	{
		CreateRecipe()
			.AddRecipeGroup("IronBar", 5)
			.AddIngredient(ItemID.GoldBar, 5)
			.AddIngredient(ItemID.Ruby, 1)
			.AddTile(TileID.WorkBenches)
			.Register();
		
		CreateRecipe()
			.AddRecipeGroup("IronBar", 5)
			.AddIngredient(ItemID.PlatinumBar, 5)
			.AddIngredient(ItemID.Ruby, 1)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
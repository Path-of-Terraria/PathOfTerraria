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
			.AddIngredient(ItemID.IronBar, 5)
			.AddTile(TileID.WorkBenches)
			.Register();
		
		CreateRecipe()
			.AddIngredient(ItemID.LeadBar, 5)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
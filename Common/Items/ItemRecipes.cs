using Terraria.ID;

namespace PathOfTerraria.Common.Items;

internal class ItemRecipes : GlobalItem
{
	public override void AddRecipes()
	{
		Recipe.Create(ItemID.Book).AddIngredient(ItemID.Wood, 1).AddTile(TileID.Sawmill).Register();
	}
}

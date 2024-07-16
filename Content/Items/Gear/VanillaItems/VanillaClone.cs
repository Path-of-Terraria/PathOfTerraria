using System.Linq;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal abstract class VanillaClone : Gear
{
	public override float DropChance => 0f;
	protected virtual short VanillaItemId => ItemID.None;
	public override string Texture => "Terraria/Images/Item_" + VanillaItemId;
	
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		GearAlternatives.Register(Type, VanillaItemId);
	}
	
	public override string GeneratePrefix()
	{
		return "";
	}

	public override string GenerateSuffix()
	{
		return "";
	}

	/// <summary>
	/// Clones the original item's recipes to the new item.
	/// </summary>
	/// <param name="originalItemId"></param>
	/// <param name="newItemId"></param>
	public static void CloneRecipes(int originalItemId, int newItemId)
	{
		foreach (Recipe recipe in Main.recipe)
		{
			if (recipe.createItem.type != originalItemId)
			{
				continue;
			}

			var newRecipe = Recipe.Create(newItemId, recipe.createItem.stack);

			foreach (Item ingredient in recipe.requiredItem)
			{
				if (ingredient != null && ingredient.type != ItemID.None)
				{
					newRecipe.AddIngredient(ingredient.type, ingredient.stack);
				}
			}

			foreach (int tile in recipe.requiredTile.Where(tile => tile != -1))
			{
				newRecipe.AddTile(tile);
			}

			newRecipe.Register();
		}
	}
}
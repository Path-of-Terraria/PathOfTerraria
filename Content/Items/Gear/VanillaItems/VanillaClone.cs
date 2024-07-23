using System.Linq;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

public abstract class VanillaClone : Gear
{
	public override float DropChance => 0f;
	protected virtual short VanillaItemId => ItemID.None;
	public override string Texture => "Terraria/Images/Item_" + VanillaItemId;

	public override LocalizedText DisplayName => Lang.GetItemName(VanillaItemId);
	public override LocalizedText Tooltip => LocalizedText.Empty;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		GearAlternatives.Register(Type, VanillaItemId);
	}

	public override void Defaults()
	{
		base.Defaults();

		Item.CloneDefaults(VanillaItemId);
	}

	public override string GeneratePrefix()
	{
		return "";
	}

	public override string GenerateSuffix()
	{
		return "";
	}

	public override void AddRecipes()
	{
		CloneRecipes(VanillaItemId, Type);
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
			recipe.DisableRecipe();
		}
	}
}
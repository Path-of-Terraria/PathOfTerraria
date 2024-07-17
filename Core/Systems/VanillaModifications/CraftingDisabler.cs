namespace PathOfTerraria.Core.Systems.VanillaModifications;

internal class CraftingDisabler : ModSystem
{
	public override void PostAddRecipes()
	{
		foreach (Recipe recipe in Main.recipe)
		{
			// Disable all relevant vanilla boss spawners
			if (BossGlobalItemDisabler.BossSpawners.Contains(recipe.createItem.type))
			{
				recipe.DisableRecipe();
			}
		}
	}
}

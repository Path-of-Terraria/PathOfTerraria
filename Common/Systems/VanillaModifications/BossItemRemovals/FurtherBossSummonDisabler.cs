using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class FurtherBossSummonDisabler : ModSystem
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

	public override void PostWorldGen()
	{
		foreach (Chest chest in Main.chest)
		{
			if (chest is null)
			{
				continue;
			}

			foreach (Item item in chest.item)
			{
				if (BossGlobalItemDisabler.BossSpawners.Contains(item.type) && !item.IsAir)
				{
					item.SetDefaults(ItemID.GoldCoin);
					item.stack = WorldGen.genRand.Next(1, 4);
				}
			}
		}
	}
}

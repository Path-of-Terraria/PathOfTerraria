using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Items.Gear;

internal class GearAlternatives
{
	internal static Dictionary<int, int> GearToVanillaAlternative = [];
	internal static Dictionary<int, int> VanillaAlternativeToGear = [];

	public static bool Register(int gear, int vanilla)
	{
		if (!GearToVanillaAlternative.ContainsKey(gear) && !VanillaAlternativeToGear.ContainsKey(vanilla))
		{
			GearToVanillaAlternative.Add(gear, vanilla);
			VanillaAlternativeToGear.Add(vanilla, gear);
			return true;
		}

		return false;
	}

	public static bool HasGear(int vanilla)
	{
		return VanillaAlternativeToGear.ContainsKey(vanilla);
	}
}

internal class GearAlternativeGlobalItem : ModSystem
{
	public override void PostAddRecipes()
	{
		foreach (Recipe recipe in Main.recipe)
		{
			if (GearAlternatives.HasGear(recipe.createItem.type))
			{
				recipe.ReplaceResult(GearAlternatives.VanillaAlternativeToGear[recipe.createItem.type], recipe.createItem.stack);
			}
		}
	}
}

internal class GearAlternativeChestReplacement : ModSystem
{
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
				if (GearAlternatives.VanillaAlternativeToGear.TryGetValue(item.type, out int value) && !item.IsAir)
				{
					item.SetDefaults(value);
					item.stack = 1;

					PoTInstanceItemData data = item.GetInstanceData();
					data.Rarity = ItemRarity.Magic;

					if (WorldGen.genRand.NextBool(10))
					{
						data.Rarity = ItemRarity.Rare;
					}

					PoTItemHelper.ClearAffixes(item);
					PoTItemHelper.Roll(item, PoTItemHelper.PickItemLevel());
				}
			}
		}
	}
}
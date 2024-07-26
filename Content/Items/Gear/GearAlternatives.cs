using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear.VanillaItems;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.VanillaModifications.BossItemRemovals;
using Terraria.DataStructures;
using Terraria.ID;

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

internal class GearAlternativeGlobalItem : GlobalItem
{
	public override void OnCreated(Item item, ItemCreationContext context)
	{
		if (context is RecipeItemCreationContext) // If crafted && a vanilla item
		{
			if (item.ModItem is null && GearAlternatives.HasGear(item.type)) //Is Vanilla Item
			{
				item.SetDefaults(GearAlternatives.VanillaAlternativeToGear[item.type]);
			}

			/*if (item.ModItem is VanillaClone)
			{
				item.CloneDefaults(GearAlternatives.GearToVanillaAlternative[item.type]);
			}*/
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
					data.Rarity = Rarity.Magic;

					if (WorldGen.genRand.NextBool(10))
					{
						data.Rarity = Rarity.Rare;
					}

					PoTItemHelper.ClearAffixes(item);
					PoTItemHelper.Roll(item, PoTItemHelper.PickItemLevel());
				}
			}
		}
	}
}
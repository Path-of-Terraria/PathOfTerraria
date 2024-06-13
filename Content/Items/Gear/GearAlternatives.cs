using System.Collections.Generic;
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

internal class GearAlternativeGlobalItem : GlobalItem
{
	public override void OnCreated(Item item, ItemCreationContext context)
	{
		if (context is RecipeItemCreationContext && item.ModItem is null) // If crafted && a vanilla item
		{
			if (GearAlternatives.HasGear(item.type))
			{
				item.SetDefaults(GearAlternatives.VanillaAlternativeToGear[item.type]);
			}
		}
	}
}

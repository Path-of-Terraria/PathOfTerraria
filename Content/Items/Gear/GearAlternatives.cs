using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear.VanillaItems;
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
		if (context is RecipeItemCreationContext) // If crafted && a vanilla item
		{
			if (item.ModItem is null && GearAlternatives.HasGear(item.type)) //Is Vanilla Item
			{
				item.SetDefaults(GearAlternatives.VanillaAlternativeToGear[item.type]);
			}

			if (item.ModItem is null)
			{
				return;
			}

			Type type = item.ModItem.GetType();
			if (!type.IsAbstract && type.IsSubclassOf(typeof(VanillaClone)))
			{
				item.SetDefaults(GearAlternatives.GearToVanillaAlternative[item.type]);
			}
		}
	}
}
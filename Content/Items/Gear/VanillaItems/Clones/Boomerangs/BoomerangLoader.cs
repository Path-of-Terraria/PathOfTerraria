using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Boomerangs;

internal class BoomerangLoader : ModSystem
{
	public override void Load()
	{
		// Prehardmode
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.EnchantedBoomerang, ItemType.Ranged, "EnchantedBoomerang"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.FruitcakeChakram, ItemType.Ranged, "FruitcakeChakram"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.BloodyMachete, ItemType.Ranged, "BloodyMachete"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Shroomerang, ItemType.Ranged, "Shroomerang"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.IceBoomerang, ItemType.Ranged, "IceBoomerang"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.ThornChakram, ItemType.Ranged, "ThornChakram"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.CombatWrench, ItemType.Ranged, "CombatWrench"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Trimarang, ItemType.Ranged, "Trimarang"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Flamarang, ItemType.Ranged, "Flamarang"));

		// Hardmode
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.FlyingKnife, ItemType.Ranged, "FlyingKnife"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.BouncingShield, ItemType.Ranged, "SergeantUnitedShield"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.LightDisc, ItemType.Ranged, "LightDisc"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Bananarang, ItemType.Ranged, "Bananarang"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.PossessedHatchet, ItemType.Ranged, "PossessedHatchet"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.PaladinsHammer, ItemType.Ranged, "PaladinsHammer"));
	}
}

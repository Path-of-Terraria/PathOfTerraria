using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Flails;

internal class FlailLoader : ModSystem
{
	public override void Load()
	{
		// Prehardmode
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Mace, ItemType.Melee, "Mace"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.FlamingMace, ItemType.Melee, "FlamingMace"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.BallOHurt, ItemType.Melee, "BallOHurt"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.TheMeatball, ItemType.Melee, "TheMeatball"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.BlueMoon, ItemType.Melee, "BlueMoon"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Sunfury, ItemType.Melee, "Sunfury"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.ChainKnife, ItemType.Melee, "ChainKnife"));

		// Hardmode
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.DripplerFlail, ItemType.Ranged, "DripplerCrippler"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.DaoofPow, ItemType.Ranged, "DaoOfPow"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.FlowerPow, ItemType.Ranged, "FlowerPow"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Anchor, ItemType.Ranged, "Anchor"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.ChainGuillotines, ItemType.Ranged, "ChainGuillotines"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Flairon, ItemType.Ranged, "Flairon"));
	}
}

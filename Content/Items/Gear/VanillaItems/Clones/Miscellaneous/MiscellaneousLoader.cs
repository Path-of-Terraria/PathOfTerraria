using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class MiscellaneousLoader : ModSystem
{
	public override void Load()
	{
		// Prehardmode
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Terragrim, ItemType.Ranged, "Terragrim"));

		// Hardmode
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.Arkhalis, ItemType.Ranged, "Arkhalis"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.JoustingLance, ItemType.Ranged, "JoustingLance"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.ShadowFlameKnife, ItemType.Ranged, "ShadowFlameKnife"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.HallowJoustingLance, ItemType.Ranged, "HallowedJoustingLance"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.MonkStaffT1, ItemType.Ranged, "SleepyOctopod"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.ScourgeoftheCorruptor, ItemType.Ranged, "ScourgeOfTheCorruptor"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.ShadowJoustingLance, ItemType.Ranged, "ShadowJoustingLance"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.MonkStaffT3, ItemType.Ranged, "SkyDragonsFury"));
		PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(ItemID.DayBreak, ItemType.Ranged, "Daybreak"));
	}
}

using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class MiscellaneousLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.SolarEruption, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.PiercingStarlight, ItemType.Melee);
		ItemDatabase.RegisterVanillaItem(ItemID.VampireKnives, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Zenith, ItemType.Melee);

		// Prehardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Terragrim, ItemType.Ranged);

		// Hardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Arkhalis, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.JoustingLance, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ShadowFlameKnife, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.HallowJoustingLance, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.MonkStaffT1, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ScourgeoftheCorruptor, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ShadowJoustingLance, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.MonkStaffT3, ItemType.Ranged);
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.DayBreak, ItemType.Ranged);
	}
}

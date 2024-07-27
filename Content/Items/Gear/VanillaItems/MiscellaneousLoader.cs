using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class MiscellaneousLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.SolarEruption, ItemType.Spear);
		ItemDatabase.RegisterVanillaItem(ItemID.PiercingStarlight, ItemType.Sword);
		ItemDatabase.RegisterVanillaItem(ItemID.VampireKnives, ItemType.Ranged); // TODO
		ItemDatabase.RegisterVanillaItem(ItemID.Zenith, ItemType.Sword);

		// Prehardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Terragrim, ItemType.Ranged); // TODO

		// Hardmode
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.Arkhalis, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.JoustingLance, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ShadowFlameKnife, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.HallowJoustingLance, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.MonkStaffT1, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ScourgeoftheCorruptor, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.ShadowJoustingLance, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.MonkStaffT3, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItem(ItemID.DayBreak, ItemType.Ranged); // TODO
	}
}

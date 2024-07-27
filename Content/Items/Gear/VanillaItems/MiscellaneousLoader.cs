using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class MiscellaneousLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.SolarEruption, ItemType.Spear);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.PiercingStarlight, ItemType.Sword);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.VampireKnives, ItemType.Ranged); // TODO
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Zenith, ItemType.Sword);

		// Prehardmode
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Terragrim, ItemType.Ranged); // TODO

		// Hardmode
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.Arkhalis, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.JoustingLance, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.ShadowFlameKnife, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.HallowJoustingLance, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.MonkStaffT1, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.ScourgeoftheCorruptor, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.ShadowJoustingLance, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.MonkStaffT3, ItemType.Ranged); // TODO
		ItemDatabase.RegisterUniqueVanillaItemAsGear(ItemID.DayBreak, ItemType.Ranged); // TODO
	}
}

using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class GunsLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Boomstick, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.ChainGun, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Gatligator, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Megashark, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Minishark, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.OnyxBlaster, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.PewMaticHorn, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.QuadBarrelShotgun, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.SDMG, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Shotgun, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.SniperRifle, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.TacticalShotgun, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Uzi, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.VenusMagnum, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.VortexBeater, ItemType.Gun);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Xenopopper, ItemType.Gun);

		// Prehardmode
		LoadGun(ItemID.RedRyder, "RedRyder");
		LoadGun(ItemID.FlintlockPistol, "FlintlockPistol");
		LoadGun(ItemID.Musket, "Musket");
		LoadGun(ItemID.TheUndertaker, "TheUndertaker");
		LoadGun(ItemID.Revolver, "Revolver");
		LoadGun(ItemID.Handgun, "Handgun");
		LoadGun(ItemID.PhoenixBlaster, "PhoenixBlaster");

		// Hardmode
		LoadGun(ItemID.ClockworkAssaultRifle, "ClockworkAssaultRifle");
		LoadGun(ItemID.CandyCornRifle, "CandyCornRifle");

		void LoadGun(short itemId, string name)
		{
			ItemDatabase.RegisterUniqueVanillaItemAsGear(itemId, ItemType.Gun);
		}
	}
}

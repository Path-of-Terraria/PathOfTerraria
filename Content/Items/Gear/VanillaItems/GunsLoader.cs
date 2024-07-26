using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class GunsLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.Boomstick, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.ChainGun, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.Gatligator, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.Megashark, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.Minishark, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.OnyxBlaster, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.PewMaticHorn, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.QuadBarrelShotgun, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.SDMG, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.Shotgun, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.SniperRifle, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.TacticalShotgun, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.Uzi, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.VenusMagnum, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.VortexBeater, ItemType.Gun);
		ItemDatabase.RegisterVanillaItem(ItemID.Xenopopper, ItemType.Gun);

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
			ItemDatabase.RegisterUniqueVanillaItem(itemId, ItemType.Gun);
		}
	}
}

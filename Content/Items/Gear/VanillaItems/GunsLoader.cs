using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class GunsLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.Boomstick, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.ChainGun, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Gatligator, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Megashark, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Minishark, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.OnyxBlaster, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.PewMaticHorn, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.QuadBarrelShotgun, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.SDMG, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Shotgun, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.SniperRifle, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.TacticalShotgun, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Uzi, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.VenusMagnum, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.VortexBeater, ItemType.Ranged);
		ItemDatabase.RegisterVanillaItem(ItemID.Xenopopper, ItemType.Ranged);

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
			ItemDatabase.RegisterUniqueVanillaItem(itemId, ItemType.Ranged);
		}
	}
}

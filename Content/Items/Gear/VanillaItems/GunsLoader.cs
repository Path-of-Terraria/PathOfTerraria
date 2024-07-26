using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones;

internal class GunsLoader : ModSystem
{
	public override void Load()
	{
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

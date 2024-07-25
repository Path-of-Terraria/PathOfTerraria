using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Guns;

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
			PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(itemId, ItemType.Ranged, name));
		}
	}
}

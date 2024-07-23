using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Launchers;

internal class LauncherLoader : ModSystem
{
	public override void Load()
	{
		LoadLauncher(ItemID.GrenadeLauncher, "GrenadeLauncher");
		LoadLauncher(ItemID.ProximityMineLauncher, "ProximityMineLauncher");
		LoadLauncher(ItemID.RocketLauncher, "RocketLauncher");
		LoadLauncher(ItemID.NailGun, "NailGun");
		LoadLauncher(ItemID.Stynger, "Stynger");
		LoadLauncher(ItemID.JackOLanternLauncher, "JackOLanternLauncher");
		LoadLauncher(ItemID.SnowmanCannon, "SnowmanCannon");
		LoadLauncher(ItemID.ElectrosphereLauncher, "ElectrosphereLauncher");
		LoadLauncher(ItemID.Celeb2, "CelebrationMK2");

		void LoadLauncher(short itemId, string name)
		{
			PoTItem.ManuallyLoadPoTItem(Mod, new InstancedVanillaClone(itemId, ItemType.Ranged, name));
		}
	}
}

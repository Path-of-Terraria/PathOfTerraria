using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class LauncherLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItem(ItemID.FireworksLauncher, ItemType.Launcher);
		ItemDatabase.RegisterVanillaItem(ItemID.Celeb2, ItemType.Launcher);

		LoadLauncher(ItemID.GrenadeLauncher, "GrenadeLauncher");
		LoadLauncher(ItemID.ProximityMineLauncher, "ProximityMineLauncher");
		LoadLauncher(ItemID.RocketLauncher, "RocketLauncher");
		LoadLauncher(ItemID.NailGun, "NailGun");
		LoadLauncher(ItemID.Stynger, "Stynger");
		LoadLauncher(ItemID.JackOLanternLauncher, "JackOLanternLauncher");
		LoadLauncher(ItemID.SnowmanCannon, "SnowmanCannon");
		LoadLauncher(ItemID.ElectrosphereLauncher, "ElectrosphereLauncher");

		void LoadLauncher(short itemId, string name)
		{
			ItemDatabase.RegisterUniqueVanillaItem(itemId, ItemType.Launcher);
		}
	}
}

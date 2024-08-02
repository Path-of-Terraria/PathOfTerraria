using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

internal class LauncherLoader : ModSystem
{
	public override void Load()
	{
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.FireworksLauncher, ItemType.Launcher);
		ItemDatabase.RegisterVanillaItemAsGear(ItemID.Celeb2, ItemType.Launcher);

		LoadLauncher(ItemID.GrenadeLauncher);
		LoadLauncher(ItemID.ProximityMineLauncher);
		LoadLauncher(ItemID.RocketLauncher);
		LoadLauncher(ItemID.NailGun);
		LoadLauncher(ItemID.Stynger);
		LoadLauncher(ItemID.JackOLanternLauncher);
		LoadLauncher(ItemID.SnowmanCannon);
		LoadLauncher(ItemID.ElectrosphereLauncher);

		static void LoadLauncher(short itemId)
		{
			ItemDatabase.RegisterUniqueVanillaItemAsGear(itemId, ItemType.Launcher);
		}
	}
}

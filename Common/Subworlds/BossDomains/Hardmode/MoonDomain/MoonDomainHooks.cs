using SubworldLibrary;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonDomainHooks : ModSystem
{
	private static bool DroppingTileItem = false;

	public override void Load()
	{
		On_Item.NewItem_Inner += BlockDropsInThisDomain;
		On_WorldGen.KillTile_DropItems += StopDrops;
		On_WorldGen.KillWall_DropItems += StopWallDrops;
	}

	private void StopWallDrops(On_WorldGen.orig_KillWall_DropItems orig, int i, int j, Tile tileCache)
	{
		DroppingTileItem = true;
		orig(i, j, tileCache);
		DroppingTileItem = false;
	}

	private void StopDrops(On_WorldGen.orig_KillTile_DropItems orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops, bool includeAllModdedLargeObjectDrops)
	{
		DroppingTileItem = true;
		orig(x, y, tileCache, includeLargeObjectDrops, includeAllModdedLargeObjectDrops);
		DroppingTileItem = false;
	}

	private int BlockDropsInThisDomain(On_Item.orig_NewItem_Inner orig, IEntitySource source, int X, int Y, int Width, int Height, Item itemToClone, int Type, int Stack,
		bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
	{
		int item = orig(source, X, Y, Width, Height, itemToClone, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);

		if (SubworldSystem.Current is not MoonLordDomain || !DroppingTileItem)
		{
			return item;
		}

		Main.item[item].active = false;
		return item;
	}
}

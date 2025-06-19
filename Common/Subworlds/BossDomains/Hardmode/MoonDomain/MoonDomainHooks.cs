using SubworldLibrary;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonDomainHooks : ModSystem
{
	private static bool DroppingTileItem = false;

	public override void Load()
	{
		On_Item.NewItem_Inner += BlockDropsInThisDomain;
		On_WorldGen.KillWall_DropItems += StopWallDrops;
		On_WorldGen.KillTile += StopAllKillTileDrops;
		On_WorldGen.TileFrame += On_WorldGen_TileFrame;
	}

	private void On_WorldGen_TileFrame(On_WorldGen.orig_TileFrame orig, int i, int j, bool resetFrame, bool noBreak)
	{
		DroppingTileItem = true;
		orig(i, j, resetFrame, noBreak);
		DroppingTileItem = false;
	}

	private void StopAllKillTileDrops(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
	{
		DroppingTileItem = true;
		orig(i, j, fail, effectOnly, SubworldSystem.Current is MoonLordDomain || noItem);
		DroppingTileItem = false;
	}

	private void StopWallDrops(On_WorldGen.orig_KillWall_DropItems orig, int i, int j, Tile tileCache)
	{
		DroppingTileItem = true;
		orig(i, j, tileCache);
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

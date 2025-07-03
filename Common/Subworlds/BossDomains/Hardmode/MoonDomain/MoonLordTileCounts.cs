using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonLordTileCounts : ModSystem
{
	public static bool SolarTiles => TileCounts[0] > 80;
	public static bool NebulaTiles => TileCounts[1] > 80;
	public static bool VortexTiles => TileCounts[2] > 80;
	public static bool StardustTiles => TileCounts[3] > 80;

	private static readonly int[] TileCounts = new int[4];

	public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
	{
		TileCounts[0] = tileCounts[TileID.SolarBrick] + tileCounts[TileID.LunarBlockSolar];
		TileCounts[1] = tileCounts[TileID.NebulaBrick] + tileCounts[TileID.LunarBlockNebula];
		TileCounts[2] = tileCounts[TileID.VortexBrick] + tileCounts[TileID.LunarBlockVortex];
		TileCounts[3] = tileCounts[TileID.StardustBrick] + tileCounts[TileID.LunarBlockStardust];
	}
}

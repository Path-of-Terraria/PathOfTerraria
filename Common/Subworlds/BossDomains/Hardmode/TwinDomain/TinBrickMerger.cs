using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.TwinDomain;

internal class TinBrickMerger : ModSystem
{
	public override void SetStaticDefaults()
	{
		Main.tileMerge[TileID.TinPlating][TileID.TinBrick] = true;
		Main.tileMerge[TileID.TinBrick][TileID.TinPlating] = true;
		Main.tileBrick[TileID.TinPlating] = true;
	}
}

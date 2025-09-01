using PathOfTerraria.Content.Tiles.BossDomain.Mech;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.SkelePrimeDomain;

internal class PassthroughTileSystem : ModSystem
{
	public override void PreUpdateProjectiles()
	{
		Main.tileSolid[ModContent.TileType<LaserBlock>()] = false;
	}

	public override void PostUpdateProjectiles()
	{
		Main.tileSolid[ModContent.TileType<LaserBlock>()] = true;
	}
}

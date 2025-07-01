using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain.Moon;

internal class Stardust : GasTile
{
	public override Color TileColor => new(22, 173, 254);
	public override Color GlowColor => new(120, 120, 250);
	public override int Dust => DustID.YellowStarDust;
}
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.HellEvent;

internal class HellEventTile : GlobalTile
{
	public override void NearbyEffects(int i, int j, int type, bool closer)
	{
		if (j > Main.maxTilesY - 210 && HellEventSystem.EventOccuring && type == TileID.Ash && !WorldGen.SolidOrSlopedTile(i, j + 1) && (Main.GameUpdateCount + i + j) % 15 == 0 && 
			Main.rand.NextBool(2, 5))
		{
			Dust.NewDustPerfect(new Vector2(i, j).ToWorldCoordinates(8, 16), DustID.Ash, new Vector2(0, Main.rand.NextFloat(1, 3)).RotatedByRandom(0.7f));
		}
	}
}

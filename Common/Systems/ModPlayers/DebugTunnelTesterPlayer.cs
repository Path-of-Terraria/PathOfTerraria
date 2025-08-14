using PathOfTerraria.Common.World.Generation;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// This is a class used to debug tunnels.<br/>This was originally used to make sure <see cref="Tunnel.GeneratePoints(Vector2[], int, float, float)"/> worked properly,<br/>
/// but it can also be used to test tunnel "shapes" on a whim to see how a preset works. This is why it's being kept.
/// </summary>
internal class DebugTunnelTesterPlayer : ModPlayer
{
	public override bool IsLoadingEnabled(Mod mod)
	{
		return false;
	}

	Vector2[] lastTunnel = [];
	Vector2[] lastPoints = [];
	Vector2[] lastBase = [];

	public override void UpdateEquips()
	{
		if (Main.mouseLeft && Main.GameUpdateCount % 10 == 0)
		{
			bool flip = WorldGen.genRand.NextBool(2);

			// Generate base points
			Vector2[] points = [new Vector2(250, 0),
				new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(160, 190)),
				new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(220, 250)),
				new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(280, 310)),
				new Vector2(-50, 150),
				new Vector2(50, 148),
				new Vector2(-50, 146),
				new Vector2(250, 400)];

			Vector2[] tunnel = Tunnel.GeneratePoints(points, out Vector2[] basePoints, 30, 12, 0);
			lastTunnel = tunnel;
			lastPoints = points;
			lastBase = basePoints;

			SpawnDust(points, tunnel, basePoints);
		}

		if (Main.mouseRight && Main.GameUpdateCount % 10 == 0)
		{
			SpawnDust(lastPoints, lastTunnel, lastBase);
		}

		if (Main.mouseMiddle && Main.GameUpdateCount % 10 == 0)
		{
			Vector2[] tunnel = Tunnel.GeneratePoints(lastPoints, out Vector2[] basePoints, 30, 12, 0);
			SpawnDust(lastPoints, tunnel, basePoints);
		}
	}

	private static void SpawnDust(Vector2[] points, Vector2[] tunnel, Vector2[] basePoints)
	{
		foreach (Vector2 point in points)
		{
			Dust.NewDustPerfect(point + Main.MouseWorld, DustID.UltraBrightTorch, Vector2.Zero, 0, Scale: 2f).noGravity = true;
		}

		foreach (Vector2 point in tunnel)
		{
			Dust.NewDustPerfect(point + Main.MouseWorld, DustID.Torch, Vector2.Zero, 0, Scale: 3f).noGravity = true;
		}

		foreach (Vector2 point in basePoints)
		{
			Dust.NewDustPerfect(point + Main.MouseWorld + Vector2.UnitX * 500, DustID.CursedTorch, Vector2.Zero, 0, Scale: 3f).noGravity = true;
		}
	}

	static int GenerateEdgeX(ref bool flip)
	{
		flip = !flip;
		return 250 + WorldGen.genRand.Next(70, 160) * (flip ? -1 : 1);
	}
}

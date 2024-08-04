using Terraria.GameContent.RGB;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.Passes.CaveSystemPasses;
internal class CaveClearingPass() : GenPass("TerrainClear", 1)
{
	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
	{
		for (int x = 0; x < Main.maxTilesX; x++)
		{
			progress.Set(x / (float)Main.maxTilesX); // Controls the progress bar, should only be set between 0f and 1f
			for (int y = 0; y < Main.maxTilesY; y++)
			{
				Vector2 point = new(x, y);
				Tile tile = Main.tile[x, y];

				// room based clear
				foreach (CaveRoom room in CaveSystemWorld.Rooms)
				{
					Vector2 diff = point - room.Position;

					float divider = -room.Size / 3f;
					if (diff.Y < divider)
					{
						float ydiff = diff.Y - divider;
						diff.Y = ydiff * 2f + divider; // if ydiff / 2f instead of *; we get an egg.
					}

					if (diff.Length() < room.Size && diff.Y < room.Size / 2f)
					{
						tile.HasTile = false;
					}
				}

				if (!tile.HasTile)
				{
					continue;
				}

				// line based clear
				foreach (Tuple<Vector2, Vector2> line in CaveSystemWorld.Lines)
				{
					Vector2 diff = line.Item2 - line.Item1;
					Vector2 dir = Vector2.Normalize(diff);

					Vector2 lhs = point - line.Item1;

					float dotP = MathF.Max(0, MathF.Min(diff.Length(), Vector2.Dot(lhs, dir)));

					Vector2 closestPoint = line.Item1 + dir * dotP;

					float effectOnThisTile = closestPoint.Distance(point) * (0.4f + NoiseHelper.GetStaticNoise(point / 200f));

					if (effectOnThisTile < 6)
					{
						tile.HasTile = false;
					}
				}
			}
		}
	}
}

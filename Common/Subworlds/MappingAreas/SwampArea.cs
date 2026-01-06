using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using PathOfTerraria.Content.Tiles.Maps.Swamp;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas;

internal class SwampArea : MappingWorld, IExplorationWorld
{
	public const int FloorY = 400;
	public const int WaterY = FloorY + 15;
	private const int MapHeight = 800;

	public static UnifiedRandom Random => Main.rand;

	private static bool LeftSpawn = false;

	public override int Width => 2600 + 150 * Main.rand.Next(3);
	public override int Height => MapHeight;
	public override int[] WhitelistedCutTiles => [TileID.Cobweb];
	public override int[] WhitelistedMiningTiles => [TileID.CrackedBlueDungeonBrick, TileID.Cobweb];
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, SunDevourerSunEdit.Blackout > 0);

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("Terrain", GenerateTerrain)];

	private void GenerateTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Main.worldSurface = FloorY + 100;
		Main.rockLayer = FloorY + 105;

		LeftSpawn = Random.NextBool(2);
		Main.spawnTileX = LeftSpawn ? 70 : Main.maxTilesX - 70;

		FastNoiseLite noise = new(Random.Next());
		noise.SetFrequency(0.017f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
		noise.SetFractalOctaves(3);
		noise.SetFractalLacunarity(0.57f);
		noise.SetFractalGain(0.45f);
		noise.SetFractalWeightedStrength(10.71f);

		Dictionary<int, int> mapping = MapY();

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				int floor = mapping[i];
				Tile tile = Main.tile[i, j];

				if (j > floor + 60 + noise.GetNoise(i, j) * 15)
				{
					tile.HasTile = true;
					tile.TileType = TileID.Mudstone;
				}
				else if (j > floor + noise.GetNoise(i, 0) * 4)
				{
					tile.HasTile = true;
					tile.TileType = TileID.Mud;
				}

				if (j > WaterY)
				{
					tile.LiquidAmount = (byte)(j == WaterY + 1 ? 150 : 255);
					tile.LiquidType = LiquidID.Water;
				}
			}

			float yOff = Math.Abs(noise.GetNoise(i * 0.5f, 15000));
			float height = Math.Abs(noise.GetNoise(i, 6000) * 3);

			for (int j = (int)(WaterY - yOff); j < WaterY + height; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile)
				{
					continue;
				}

				tile.HasTile = true;
				tile.TileType = (ushort)ModContent.TileType<SwampMoss>();
			}

			progress.Set(i / (float)Main.maxTilesX);
		}

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				if (!Random.NextBool(3))
				{
					Tile.SmoothSlope(i, j, false);
				}
				
				Tile tile = Main.tile[i, j];

				if (WorldGen.TileIsExposedToAir(i, j) && tile.HasTile && tile.TileType == TileID.Mud)
				{
					tile.HasTile = true;
					tile.TileType = (ushort)ModContent.TileType<SwampGrass>();
				}
			}
		
			progress.Set(i / (float)Main.maxTilesX);
		}
	}

	private static Dictionary<int, int> MapY()
	{
		Dictionary<int, int> yPerX = [];

		int x = 400;
		List<Point16> dips = [];

		while (x < Main.maxTilesX - 250)
		{
			x++;

			if (Random.NextBool(150))
			{
				if (x < Main.maxTilesX - 400)
				{
					dips.Add(new Point16(x, FloorY + Random.Next(90, 180)));
					x += Random.Next(200);
				}
				else if (Random.NextBool(40))
				{
					dips.Add(new Point16(Main.maxTilesX - 400, FloorY + Random.Next(40, 80)));
					break;
				}
			}
		}

		float realY = FloorY;
		float rigidness = Random.NextFloat(0.25f, 0.35f);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			const float MaxDistance = 200;

			float y = FloorY;
			Point16? dipPoint = null;
			float distance = 0;
			
			foreach (Point16 point in dips)
			{
				int dist = Math.Abs(i - point.X);
				if (dist < MaxDistance && (!dipPoint.HasValue || Math.Abs(i - dipPoint.Value.X) > dist))
				{
					dipPoint = point;
					distance = dist;
				}
			}

			if (dipPoint.HasValue)
			{
				y = (int)MathHelper.Lerp(y, dipPoint.Value.Y, (float)Math.Pow(1 - distance / MaxDistance, 0.5));
			}

			realY = MathHelper.Lerp(realY, y, rigidness);

			yPerX.Add(i, (int)realY);
		}

		return yPerX;
	}

	public override void Update()
	{
		Liquid.UpdateLiquid();
	}
}

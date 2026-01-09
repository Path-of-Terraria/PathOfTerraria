using PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;
using PathOfTerraria.Common.Tiles.FramingKinds;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using PathOfTerraria.Content.Tiles.Maps.Swamp;
using PathOfTerraria.Utilities;
using System.Collections.Generic;
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
	public const int FloorY = 550;
	public const int WaterY = FloorY + 10;
	private const int MapHeight = 900;

	public static UnifiedRandom Random => Main.rand;

	private static bool LeftSpawn = false;

	public override int Width => 3000 + 150 * Main.rand.Next(3);
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

		FastNoiseLite perlin = new(Random.Next());

		Dictionary<int, int> mapping = MapY();

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				int floor = mapping[i];
				Tile tile = Main.tile[i, j];

				if (j > floor + 80 + noise.GetNoise(i, j) * 15)
				{
					tile.HasTile = true;
					tile.TileType = TileID.Mudstone;
				}
				else if (j > floor + perlin.GetNoise(i * 2.5f, 0) * 4)
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

			progress.Set(i / (float)Main.maxTilesX);
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.TreesAndLeaves");

		Queue<int> mangroves = [];

		for (int i = 0; i < 2; ++i)
		{
			int index;

			do
			{
				index = Random.Next(2, Main.maxTilesX / 300 - 2);
			} while (mangroves.Contains(index));

			mangroves.Enqueue(index);
		}

		while (mangroves.Count > 0)
		{
			BranchTreeMicrobiome biome = new();
			biome.Place(new(mangroves.Dequeue() * 300, FloorY - Random.Next(150, 340)), GenVars.structures ?? new());
		}

		PriorityQueue<Point, float> cypresses = new();

		for (int i = 1; i < Main.maxTilesX / 200; ++i)
		{
			if (WorldUtils.Find(new(i * 200, FloorY), new Searches.Down(600).Conditions(new Conditions.IsSolid()), out Point result))
			{
				Tile tile = Main.tile[result.X, result.Y - 4];

				if (tile.LiquidAmount > 0 && GenVars.structures.CanPlace(new Rectangle(result.X - 30, result.Y - 30, 60, 60)))
				{
					cypresses.Enqueue(new Point(result.X, result.Y - 4), Random.NextFloat());
				}
			}
		}

		int cypressCount = Math.Min(8, cypresses.Count);
		CypressTreeMicrobiome cypressBiome = new();
		Action delayment = () => { };

		for (int i = 0; i < cypressCount; ++i)
		{
			Point origin = cypresses.Dequeue();

			delayment += () => cypressBiome.Place(origin, GenVars.structures);
			PlaceExtraCypress(cypressBiome, origin, 1, ref delayment, 2, 0.8f);
			PlaceExtraCypress(cypressBiome, origin, -1, ref delayment, 2, 0.8f);
		}

		delayment.Invoke();
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		for (int i = 1; i < Main.maxTilesX - 2; ++i)
		{
			if (Random.NextBool(70) && !Main.tile[i, WaterY + 1].HasTile)
			{
				ILilyPadTile.PlacePad<SwampPad>(i, WaterY + 1, false);
			}

			progress.Set(i / (float)Main.maxTilesX);
		}

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				if (!Random.NextBool(3) && WorldGen.TileIsExposedToAir(i, j))
				{
					Tile.SmoothSlope(i, j, false);
				}
				
				Tile tile = Main.tile[i, j];

				if (WorldGen.TileIsExposedToAir(i, j) && tile.HasTile && tile.TileType == TileID.Mud)
				{
					tile.HasTile = true;
					tile.TileType = (ushort)ModContent.TileType<SwampGrass>();

					if (!Random.NextBool(4))
					{
						WorldGen.PlaceTile(i, j - 1, ModContent.TileType<SwampPlants1x1>(), true, false, -1, Random.Next(6));
					}
				}

				if (WorldUtilities.SolidTile(i, j) && WorldUtilities.TileOrphaned(i, j))
				{
					tile.HasTile = false;
				}
			}
		
			progress.Set(i / (float)Main.maxTilesX);
		}
	}

	private static void PlaceExtraCypress(CypressTreeMicrobiome cypressBiome, Point origin, int direction, ref Action delayment, int chance, float sizeMod)
	{
		if (sizeMod <= 0.4f)
		{
			return;
		}

		int offsetX = origin.X - (int)(Random.Next(80, 160) * direction * sizeMod);
		ushort grass = (ushort)ModContent.TileType<SwampGrass>();

		if (Random.NextBool(chance) && WorldUtils.Find(new(offsetX, FloorY), new Searches.Down(600).Conditions(new Conditions.IsSolid()), out Point result))
		{
			delayment += () =>
			{
				using var _ = ValueOverride.Create(ref CypressTreeMicrobiome.SizeModifier, Random.NextFloat(sizeMod * 0.75f, sizeMod));
				cypressBiome.Place(result, GenVars.structures);
			};

			PlaceExtraCypress(cypressBiome, result, direction, ref delayment, chance * chance, sizeMod * 0.7f);
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
		float rigidness = Random.NextFloat(0.25f, 0.5f);

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

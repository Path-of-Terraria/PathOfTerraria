using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;
using PathOfTerraria.Common.Tiles.FramingKinds;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using PathOfTerraria.Content.NPCs.Mapping.Swamp;
using PathOfTerraria.Content.Tiles.Maps.Swamp;
using PathOfTerraria.Content.Walls;
using PathOfTerraria.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas;

#nullable enable

internal class SwampArea : MappingWorld, IExplorationWorld, IOverrideBiome
{
	public const int FloorY = 470;
	public const int WaterY = FloorY + 10;
	public const int CloudLayer = 180;
	public const int MapHeight = 730;
	public const float MossNoiseThreshold = -0.4f;

	public static UnifiedRandom Random => Main.rand;

	internal static Dictionary<int, int>? HeightMapping = null;
	internal static List<Vector2> EncounterLocations = [];
	internal static HashSet<Point16> SkipActuationLocations = [];

	private bool LeftSpawn = false;
	private bool spawnedTemporaryContent = false;

	public override int Width => 3000 + 200 * Main.rand.Next(3);
	public override int Height => MapHeight;
	public override int[] WhitelistedCutTiles => [TileID.Cobweb];
	public override int[] WhitelistedMiningTiles => [TileID.CrackedBlueDungeonBrick, TileID.Cobweb];
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, SunDevourerSunEdit.Blackout > 0);

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("Terrain", GenerateTerrain), new PassLegacy("SettleLiquids", SettleLiquidsStep.Generation)];

	private void GenerateTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");
		progress.CurrentPassWeight = 1;

		Main.worldSurface = WaterY + 10;
		Main.rockLayer = WaterY + 40;

		spawnedTemporaryContent = false;
		EncounterLocations.Clear();
		LeftSpawn = Random.NextBool(2);
		SkipActuationLocations.Clear();

		Main.spawnTileX = LeftSpawn ? 70 : Main.maxTilesX - 70;
		Main.spawnTileY = FloorY - 6;

		FastNoiseLite noise = new(Random.Next());
		noise.SetFrequency(0.017f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
		noise.SetFractalOctaves(3);
		noise.SetFractalLacunarity(0.57f);
		noise.SetFractalGain(0.45f);
		noise.SetFractalWeightedStrength(10.71f);

		FastNoiseLite perlin = new(Random.Next());

		Dictionary<int, int> mapping = MapTerrainHeight();
		HeightMapping = mapping;

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				int floor = mapping[i];
				Tile tile = Main.tile[i, j];

				if (j > floor + 80 + noise.GetNoise(i, j) * 15)
				{
					tile.HasTile = true;
					tile.TileType = TileID.Stone;
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

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		List<Point16> grasses = [];

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (WorldGen.TileIsExposedToAir(i, j) && tile.HasTile && tile.TileType == TileID.Mud)
				{
					tile.HasTile = true;
					tile.TileType = (ushort)ModContent.TileType<SwampGrass>();

					grasses.Add(new Point16(i, j));
				}

				if (WorldUtilities.SolidTile(i, j) && WorldUtilities.TileOrphaned(i, j))
				{
					tile.HasTile = false;
				}
			}

			progress.Set(i / (float)Main.maxTilesX);
		}

		GrowGrasses(grasses, perlin);

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.TreesAndLeaves");

		GrowTrees();
		GrowDeepMoss(progress);
		GrowLilyPads(progress);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			int topY = FloorY - 10 - (int)(Math.Abs(noise.GetNoise(i, 10000)) * 10);

			for (int j = topY; j < Main.maxTilesY - 40; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile || !WorldUtilities.SolidOrActuatedTile(tile) || SkipActuationLocations.Contains(new Point16(i, j)))
				{
					continue;
				}

				bool actuate = j > HeightMapping[i] - Math.Abs(noise.GetNoise(i, j) * 6) - 10 && j <= HeightMapping[i] - 5;

				if (!actuate)
				{
					actuate = j > HeightMapping[i] - Math.Abs(noise.GetNoise(i, j) * 15) - 40 && j <= HeightMapping[i] - 30;
				}

				tile.IsActuated = actuate;
			}
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Clouds");
		FastNoiseLite cloudNoise = GenerateClouds(progress);

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Growing");
		CleanUpAndDetail(progress, cloudNoise);
	}

	private static FastNoiseLite GenerateClouds(GenerationProgress progress)
	{
		FastNoiseLite cloudNoise = new(Random.Next());
		cloudNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
		cloudNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		cloudNoise.SetFractalLacunarity(3.85f);
		cloudNoise.SetFractalGain(0.39f);
		cloudNoise.SetFractalWeightedStrength(3.92f);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			const int Distance = 100;

			int bottomY = (int)(CloudLayer + cloudNoise.GetNoise(i * 0.5f, 3000) * 90);

			for (int j = 2; j < CloudLayer + Distance; ++j)
			{
				float value = MathHelper.Lerp(cloudNoise.GetNoise(i, j), 0.2f, j > bottomY - Distance ? Utils.GetLerpValue(bottomY - Distance, bottomY, j, true) : 0);

				if (value <= 0)
				{
					Tile tile = Main.tile[i, j];
					tile.TileType = (ushort)ModContent.TileType<PurpleClouds>();
					tile.HasTile = true;
				}

				value = MathHelper.Lerp(cloudNoise.GetNoise(i, j + 12000), 0.2f, j > bottomY - Distance ? Utils.GetLerpValue(bottomY - Distance, bottomY, j, true) : 0);

				if (value <= 0)
				{
					GenPlacement.FastPlaceWall(i, j, ModContent.WallType<PurpleCloudWall>());
				}
			}

			progress.Set(i / (float)Main.maxTilesX);
		}
		
		return cloudNoise;
	}

	private static void GrowDeepMoss(GenerationProgress progress)
	{
		FastNoiseLite noise = new(Random.Next());
		noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		noise.SetFrequency(0.05f);

		for (int i = 300; i < Main.maxTilesX - 300; ++i)
		{
			for (int j = WaterY + 20; j < Main.maxTilesY - 60; ++j)
			{
				if (noise.GetNoise(i, j) > MossNoiseThreshold && !WorldUtilities.SolidOrActuatedTile(i, j))
				{
					GenPlacement.FastPlaceTile(i, j, ModContent.TileType<DeepMoss>());
				}
			}

			progress.Set(i / (float)Main.maxTilesX);
		}
	}

	private static void CleanUpAndDetail(GenerationProgress progress, FastNoiseLite cloudNoise)
	{
		List<Point16> leaves = [];

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];
				bool touchingAir = WorldGen.TileIsExposedToAir(i, j);

				if (!Random.NextBool(3) && touchingAir && (Main.tile[i, j - 1].LiquidAmount <= 0 || tile.TileType != ModContent.TileType<SwampGrass>()))
				{
					Tile.SmoothSlope(i, j, false);
				}

				if (WorldUtilities.SolidUnslopedTile(i, j))
				{
					if (tile.TileType == TileID.LeafBlock)
					{
						if (!WorldUtilities.SolidTile(i, j + 1))
						{
							bool isWall = Random.NextBool(3);
							int length = isWall ? Random.Next(7, 23) : Random.Next(3, 12);

							for (int v = isWall ? -1 : 1; v < length; ++v)
							{
								if (!isWall && WorldUtilities.SolidTile(Main.tile[i, j + v]))
								{
									break;
								}

								if (isWall)
								{
									GenPlacement.FastPlaceWall(i, j + v, WallID.LivingLeaf);
								}
								else
								{
									GenPlacement.FastPlaceTile(i, j + v, TileID.Vines);
								}
							}
						}

						if (touchingAir)
						{
							leaves.Add(new Point16(i, j));
						}
					}

					if (!WorldUtilities.SolidTile(i, j - 1) && (tile.TileType == ModContent.TileType<SwampGrass>() || tile.TileType == ModContent.TileType<DeepMoss>()))
					{
						int height = (int)(cloudNoise.GetNoise(i * 3, j) * 260) + 2 + Random.Next(5);

						if (tile.TileType == ModContent.TileType<DeepMoss>())
						{
							height /= 2;
						}

						for (int v = 1; v < height; ++v)
						{
							int frame = 0;

							if (v < height / 3)
							{
								frame = 2;
							}
							else if (v < height / 3 * 2)
							{
								frame = 1;
							}

							SwampWeed.Place(i, j - v, frame);
						}
					}
				}
			}

			progress.Set(i / (float)Main.maxTilesX);
		}

		for (int i = 0; i < 150; ++i)
		{
			Point16 firstPoint = Random.Next(leaves);
			Vector2 one = firstPoint.ToVector2() + OpenExtensions.GetDirectionRandom(OpenExtensions.GetUnsolidOpenings(firstPoint.X, firstPoint.Y, false, false)).ToVector2();

			Point16 secondPoint;

			do
			{
				secondPoint = Random.Next(leaves);
			} while (firstPoint == secondPoint || one.DistanceSQ(secondPoint.ToVector2()) > 700 * 700);

			Vector2 two = secondPoint.ToVector2() + OpenExtensions.GetDirectionRandom(OpenExtensions.GetUnsolidOpenings(secondPoint.X, secondPoint.Y, false, false)).ToVector2();
			float distance = two.Distance(one);
			Vector2[] vine = Tunnel.GenerateBezier([one, Vector2.Lerp(one, two, 0.5f) + new Vector2(0, Random.NextFloat(distance * 0.33f, distance * 0.5f)), two], 0.8f, 0);
			float baseStrength = Random.NextFloat(0.8f, 1.5f) * (distance / 500f);

			for (int j = 0; j < vine.Length; j++)
			{
				Vector2 pos = vine[j];
				
				if (pos.Y > WaterY)
				{
					float dif = pos.Y - WaterY;
					pos.Y = WaterY + dif / 4f;
				}

				float str = MathF.Abs(j - vine.Length / 2) / (vine.Length / 2f);
				GenPlacement.WallCircle(pos, baseStrength * str + 1.05f, WallID.LivingLeaf);

				if (Random.NextBool(10))
				{
					leaves.Add(pos.ToPoint16());
				}

				if (Random.NextBool(80))
				{
					float length = str * Random.NextFloat(6f, 35f);

					for (int v = 0; v < length; ++v)
					{
						GenPlacement.WallCircle(pos + new Vector2(0, v), 1.5f - (v / length), WallID.LivingLeaf);

						if (Random.NextBool(8))
						{
							leaves.Add(new Point16((int)pos.X, (int)pos.Y + v));
						}
					}
				}
			}
		}

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (j > WaterY)
				{
					tile.LiquidAmount = (byte)(j == WaterY + 1 ? 150 : 255);
					tile.LiquidType = LiquidID.Water;
				}
			}

			progress.Set(i / (float)Main.maxTilesX);
		}
	}

	private static void GrowLilyPads(GenerationProgress progress)
	{
		for (int i = 1; i < Main.maxTilesX - 2; ++i)
		{
			if (Random.NextBool(210) && !Main.tile[i, WaterY + 1].HasTile)
			{
				ILilyPadTile.PlacePad<SwampPad>(i, WaterY + 1, false);
			}

			progress.Set(i / (float)Main.maxTilesX);
		}
	}

	private static void GrowTrees()
	{
		Queue<int> mangroves = [];

		for (int i = 0; i < 2; ++i)
		{
			int index;

			do
			{
				index = Random.Next(2, Main.maxTilesX / 300 - 2);
			} while (mangroves.Any(x => Math.Abs(x - index) < 2));

			mangroves.Enqueue(index);
		}

		while (mangroves.Count > 0)
		{
			BranchTreeMicrobiome biome = new();
			biome.Place(new(mangroves.Dequeue() * 300, FloorY - Random.Next(150, 220)), GenVars.structures ?? new());
		}

		const int CypressSpace = 140;
		PriorityQueue<Point, float> cypresses = new();

		for (int i = 1; i < Main.maxTilesX / CypressSpace; ++i)
		{
			if (WorldUtils.Find(new(i * CypressSpace, FloorY), new Searches.Down(600).Conditions(new Conditions.IsSolid()), out Point result))
			{
				Tile tile = Main.tile[result.X, result.Y - 4];

				if (tile.LiquidAmount > 0 && GenVars.structures!.CanPlace(new Rectangle(result.X - 30, result.Y - 30, 60, 60)))
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
	}

	private static void GrowGrasses(List<Point16> grasses, FastNoiseLite noise)
	{
		Dictionary<int, PriorityQueue<Point16, float>> decor = [];

		foreach (Point16 position in CollectionsMarshal.AsSpan(grasses))
		{
			position.Deconstruct(out short i, out short j);
			bool underwater = Main.tile[position].LiquidAmount > 0;

			if (!underwater && Random.NextBool(70))
			{
				decor.TryAdd(0, new());
				decor[0].Enqueue(position, Random.NextFloat());
			}
			else if (Random.NextBool(44))
			{
				decor.TryAdd(1, new());
				decor[1].Enqueue(position, Random.NextFloat());
			}
			else if (!Random.NextBool(4) && !underwater && !Main.tile[i, j - 1].HasTile)
			{
				WorldGen.PlaceTile(i, j - 1, ModContent.TileType<SwampPlants1x1>(), true, false, -1, Random.Next(6));
			}
		}

		while (decor[1].Count > 0)
		{
			Point16 boulders = decor[1].Dequeue();
			bool underwater = Main.tile[boulders].LiquidAmount > 0;
			int chance = underwater ? 4 : 2;
			int id = !Random.NextBool() ? (underwater ? ModContent.TileType<DeepMoss>() : ModContent.TileType<SwampMoss>()) : TileID.Mudstone;
			GenPlacement.GenOval(boulders.ToVector2(), Random.NextFloat(2, 8), Random.NextFloat(MathHelper.PiOver2 - 0.3f, MathHelper.PiOver2 + 0.3f) * (Random.NextBool() ? -1 : 1),
				id, (x, y) => noise.GetNoise(x, y) * 8);
		}

		while (decor[0].Count > 0)
		{
			Point16 log = decor[0].Dequeue();
			log.Deconstruct(out short i, out short j);

			while (WorldUtilities.SolidTile(i, j))
			{
				j--;
			}

			int width = Random.Next(5, 9);
			int height = Random.Next(3, 6);

			j -= (short)(height / 2);

			for (int x = i; x < i + width; ++x)
			{
				for (int y = j; y < j + height; ++y)
				{
					GenPlacement.FastPlaceTile(x, y, TileID.LivingWood).IsActuated = y != j && y != j + height - 1;
					SkipActuationLocations.Add(new Point16(x, y));
				}
			}
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

	private static Dictionary<int, int> MapTerrainHeight()
	{
		Dictionary<int, int> yPerX = [];

		int x = 400;
		int lastX = 400;
		List<Point16> dips = [];

		while (x < Main.maxTilesX - 250)
		{
			x++;

			if (Random.NextBool(150) || (x - lastX) > 400)
			{
				if (x < Main.maxTilesX - 400)
				{
					dips.Add(new Point16(x, FloorY + Random.Next(90, 180)));
					x += Random.Next(200);
					lastX = x;
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

		if (NPC.CountNPCS(ModContent.NPCType<GiantEel>()) < Main.CurrentFrameFlags.ActivePlayersCount)
		{
			NPC.NewNPC(new EntitySource_WorldGen(), Main.spawnTileX * 16, 120 * 16, ModContent.NPCType<GiantEel>(), 0, 0, 0, Main.CurrentFrameFlags.ActivePlayersCount - 1);
		}

		if (!spawnedTemporaryContent && Main.ActivePlayers.GetEnumerator().MoveNext())
		{
			PlaceEncounters();
			SpawnGenEntities();
			spawnedTemporaryContent = true;
		}
	}

	private static void SpawnGenEntities()
	{
		List<float> xPositions = [];

		for (int i = 0; i < 25; ++i)
		{
			Vector2 pos;

			do
			{
				pos = new Vector2(Random.Next(300 * 16, (Main.maxTilesX - 300) * 16), Random.Next(FloorY * 16 - 200, FloorY * 16 + 200));
			} while (Collision.SolidCollision(pos - new Vector2(50, 0), 200, 60) || !Collision.WetCollision(pos, 100, 20) || Collision.WetCollision(pos - new Vector2(0, 30), 100, 20)
				|| xPositions.Any(x => Math.Abs(x - pos.X) < 120));

			int type = Random.NextBool() ? ModContent.ProjectileType<FloatingMudplatform>() : ModContent.ProjectileType<BrittleFloatingMudplatform>();

			if (i >= 5)
			{
				Projectile.NewProjectile(new EntitySource_WorldGen(), pos, Vector2.Zero, type, 0, 0, Main.myPlayer);
			}
			else
			{
				NPC.NewNPC(new EntitySource_WorldGen(), (int)pos.X, (int)pos.Y, ModContent.NPCType<SwampCroc>(), 0, 0, 1);
			}

			xPositions.Add(pos.X);
		}
	}

	private static void PlaceEncounters()
	{
		foreach (Vector2 position in EncounterLocations)
		{
			Encounter encounter = EncounterIO.CreateEncounterFromModPath(PoTMod.Instance, "Content/Encounters/SwampMangrove");
			encounter.MoveEverythingTo(position.ToPoint16());
		}
	}

	public void OverrideBiome()
	{
		Main.bgStyle = 0;
		Main.curMusic = MusicID.OverworldDay;
	}
}

using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class PlanteraDomain : BossDomainSubworld
{
	public override int Width => 1700;
	public override int Height => 800;
	public override (int time, bool isDay) ForceTime => ((int)Main.nightLength / 2, false);
	public override int[] WhitelistedExplodableTiles => [ModContent.TileType<ExplosivePowder>()];
	public override int[] WhitelistedCutTiles => [TileID.JungleVines];
	public override int[] WhitelistedMiningTiles => [TileID.PlanteraBulb, TileID.Mud, TileID.JungleGrass, TileID.Mudstone, TileID.Stone, ModContent.TileType<BabyBulb>()];

	public static Point16 BulbPosition = new();
	public static int BulbsBroken = 0;

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain)];

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		BulbsBroken = 0;
		BossSpawned = false;
		ExitSpawned = false;

		progress.Start(1);
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Main.worldSurface = 20;
		Main.rockLayer = 30;

		Vector2 center = new(Width / 2, Height / 2);
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.03f);

		FastNoiseLite cell = new(WorldGen._genRandSeed);
		cell.SetNoiseType(FastNoiseLite.NoiseType.Value);
		cell.SetFrequency(0.06f);
		cell.SetDomainWarpType(FastNoiseLite.DomainWarpType.BasicGrid);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				float value = cell.GetNoise(i, j) - MathHelper.Lerp(noise.GetNoise(i, j), 1, 0.2f);
				Tile tile = Main.tile[i, j];
				float dist = MathTools.ModDistance(new Vector2(i, j), center, 1, 8);

				value = MathHelper.Lerp(value, 1, GetAdjustmentBasedOnDistance(dist * (1 + MathHelper.Lerp(noise.GetNoise(i, j), 0, 0.4f))));
				tile.ClearEverything();

				float wallValue = noise.GetNoise(i, j + 8000);
				float emptyWallValue = noise.GetNoise(i, j + 16000);
				tile.WallType = wallValue > 0.4f ? WallID.JungleUnsafe : emptyWallValue < 0.2f ? WallID.None : WallID.MudUnsafe;

				if (value > 0)
				{
					tile.TileType = TileID.Mud;
					tile.HasTile = true;
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		Dictionary<Point16, OpenFlags> grasses = [];
		HashSet<Point16> branches = [];

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType == TileID.Mud)
				{
					OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

					if (flags != OpenFlags.None)
					{
						tile.TileType = TileID.JungleGrass;

						grasses.Add(new Point16(i, j), flags);
					}
					else if (WorldGen.genRand.NextBool(180))
					{
						ushort type = WorldGen.genRand.NextBool() ? TileID.Mudstone : TileID.Stone;
						WorldGen.TileRunner(i, j, WorldGen.genRand.NextFloat(3, 12), WorldGen.genRand.Next(3, 12), type);
					}
					else if (WorldGen.genRand.NextBool(200) && WorldGen.InWorld(i, j, 50))
					{
						float angle = WorldGen.genRand.NextFloat(-MathHelper.TwoPi, MathHelper.TwoPi);
						var root = new ShapeRoot(angle, WorldGen.genRand.NextFloat(18, 30), WorldGen.genRand.NextFloat(4, 8), 1);
						WorldUtils.Gen(new Point(i, j), root, new Actions.PlaceWall(WallID.LivingWoodUnsafe));
					}
				}

				if (WorldGen.genRand.NextBool(350))
				{
					WallRunner(i, j, Main.rand.Next(6) switch
					{
						0 => WallID.JungleUnsafe,
						1 => WallID.JungleUnsafe1,
						2 => WallID.JungleUnsafe2,
						3 => WallID.JungleUnsafe3,
						_ => WallID.JungleUnsafe4
					});
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		HashSet<Point16> spores = [];

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile && tile.WallType != WallID.None)
				{
					if (WorldGen.genRand.NextBool(120) && !spores.Contains(new Point16(i, j)))
					{
						bool isFlower = WorldGen.genRand.NextBool(10);
						int type = isFlower ? ModContent.TileType<Seeflower>() : ModContent.TileType<GlowingSpores>();
						WorldGen.PlaceTile(i, j, type, true, false, -1, WorldGen.genRand.Next(isFlower ? 1 : 3));

						for (int k = -2; k < 3; ++k)
						{
							for (int l = -2; l < 3; ++l)
							{
								spores.Add(new Point16(i + k, j + l));
							}
						}
					}
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		foreach (KeyValuePair<Point16, OpenFlags> grass in grasses)
		{
			PlaceGrass(grass.Key.X, grass.Key.Y, grass.Value);
		}

		PlaceBulb();
	}

	internal static void PlaceBulb(bool realTime = false)
	{
		var center = new Vector2(Main.maxTilesX / 2f, Main.maxTilesY / 2f);

		while (true)
		{
			float angle = WorldGen.genRand.NextFloat(-MathHelper.TwoPi, MathHelper.TwoPi);
			Vector2 basePos = new Vector2(Main.rand.NextFloat(500), 0).RotatedBy(angle) / new Vector2(1, MathF.Sqrt(5));
			var pos = (center + basePos).ToPoint16();

			if (BulbsBroken >= 2)
			{
				WorldGen.PlaceJunglePlant(pos.X, pos.Y, TileID.PlanteraBulb, 0, 0);
			}
			else
			{
				WorldGen.PlaceObject(pos.X, pos.Y, ModContent.TileType<BabyBulb>(), true);
			}

			if (Main.tile[pos].HasTile && Main.tile[pos].TileType == (BulbsBroken >= 2 ? TileID.PlanteraBulb : ModContent.TileType<BabyBulb>()) 
				&& (!realTime || !WorldGen.PlayerLOS(pos.X, pos.Y)))
			{
				BulbPosition = pos;

				if (realTime)
				{
					NetMessage.SendTileSquare(-1, pos.X, pos.Y, 5);
				}

				break;
			}
		}
	}

	public static void WallRunner(int i, int j, int wall)
	{
		double num = WorldGen.genRand.Next(8, 21);
		double num2 = WorldGen.genRand.Next(8, 33);
		double num3 = num2;
		Vector2D basePos = new(i, j);
		Vector2D velocity = default;
		velocity.X = WorldGen.genRand.Next(-10, 11) * 0.1;
		velocity.Y = WorldGen.genRand.Next(-10, 11) * 0.1;
		
		while (num > 0.0 && num3 > 0.0)
		{
			double num4 = num * (num3 / num2);
			num3 -= 1.0;
			int num5 = (int)(basePos.X - num4 * 0.5);
			int num6 = (int)(basePos.X + num4 * 0.5);
			int num7 = (int)(basePos.Y - num4 * 0.5);
			int num8 = (int)(basePos.Y + num4 * 0.5);

			if (num5 < 0)
			{
				num5 = 0;
			}

			if (num6 > Main.maxTilesX)
			{
				num6 = Main.maxTilesX;
			}

			if (num7 < 0)
			{
				num7 = 0;
			}

			if (num8 > Main.maxTilesY)
			{
				num8 = Main.maxTilesY;
			}

			for (int k = num5; k < num6; k++)
			{
				for (int l = num7; l < num8; l++)
				{
					if (Math.Abs(k - basePos.X) + Math.Abs(l - basePos.Y) < num * 0.5 * (1.0 + WorldGen.genRand.Next(-10, 11) * 0.015) && l > Main.worldSurface)
					{
						Main.tile[k, l].WallType = (ushort)wall;
					}
				}
			}

			basePos += velocity;
			velocity.X += WorldGen.genRand.Next(-10, 11) * 0.05;
			velocity.X = Math.Clamp(basePos.X, -1, 1);
			velocity.Y += WorldGen.genRand.Next(-10, 11) * 0.05;
			velocity.Y = Math.Clamp(basePos.Y, -1, 1);
		}
	}

	private static void PlaceGrass(short x, short y, OpenFlags flags)
	{
		if (flags.HasFlag(OpenFlags.Above))
		{
			new CheckChain((int x, int y, ref int? checkType) =>
			{
				if (WorldGen.genRand.NextBool(2))
				{
					return;
				}

				checkType = 233;
				WorldGen.PlaceJunglePlant(x, y, 233, WorldGen.genRand.Next(8), 0);
			}).Chain((int x, int y, ref int? checkType) =>
			{
				if (WorldGen.genRand.NextBool(2))
				{
					return;
				}

				checkType = 233;
				WorldGen.PlaceJunglePlant(x, y, 233, WorldGen.genRand.Next(12), 1);
			}).Chain((int x, int y, ref int? checkType) =>
			{
				bool shortGrass = WorldGen.genRand.NextBool(3);
				checkType = shortGrass ? TileID.JunglePlants2 : TileID.JunglePlants;
				WorldGen.PlaceTile(x, y, checkType.Value, true, false, style: WorldGen.genRand.Next(shortGrass ? 24 : 16));

				Tile tile = Main.tile[x, y];
				tile.TileFrameX = (short)((WorldGen.genRand.NextBool(5) && shortGrass ? 8 : WorldGen.genRand.Next(24)) * 18);
			}).Run(x, y - 1);
		}

		if (flags.HasFlag(OpenFlags.Below))
		{
			if (!WorldGen.genRand.NextBool(3))
			{
				int length = WorldGen.genRand.Next(5, 12);

				for (int k = 1; k < length; ++k)
				{
					if (Main.tile[x, y + k].HasTile)
					{
						break;
					}

					WorldGen.PlaceTile(x, y + k, TileID.JungleVines, true);
				}
			}
		}
	}

	private static float GetAdjustmentBasedOnDistance(float dist)
	{
		return MathHelper.Clamp((dist - 600) / 200f, 0, 1);
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ExitSpawned = false;
	}

	public override void Update()
	{
		Wiring.UpdateMech();
		TileEntity.UpdateStart();

		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();

		if (!BossSpawned && NPC.AnyNPCs(NPCID.Plantera))
		{
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.Plantera) && !ExitSpawned)
		{
			ExitSpawned = true;

			HashSet<Player> players = [];

			foreach (Player plr in Main.ActivePlayers)
			{
				if (!plr.dead)
				{
					players.Add(plr);
				}
			}

			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Vector2 position = Main.rand.Next([.. players]).Center - new Vector2(0, 60);
			Projectile.NewProjectile(src, position, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
		}
	}
}

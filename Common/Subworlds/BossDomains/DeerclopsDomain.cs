using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria.Enums;
using PathOfTerraria.Common.World.Generation;
using Terraria.DataStructures;
using PathOfTerraria.Common.Subworlds.Passes;
using SubworldLibrary;
using Terraria.Utilities;
using System.Linq;
using PathOfTerraria.Content.Tiles.BossDomain;
using ReLogic.Content;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class DeerclopsDomain : BossDomainSubworld
{
	internal static Asset<Texture2D> LightGlow = null;

	public static int Surface => 200;

	public override int Width => 800;
	public override int Height => 800;
	public override int[] WhitelistedCutTiles => [TileID.BreakableIce];
	public override int[] WhitelistedMiningTiles => [TileID.BreakableIce, ModContent.TileType<RopeClump>(), TileID.Platforms];
	public override int[] WhitelistedPlaceableTiles => [TileID.Platforms];
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, true);

	internal static float LightMultiplier = 0;

	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), 
		new FlatWorldPass(Surface, true, GetSurfaceNoise(), TileID.SnowBlock, WallID.SnowWallUnsafe), 
		new PassLegacy("Tunnels", Tunnels),
		new PassLegacy("Polish", Polish),
		new PassLegacy("Settle Liquids", SettleLiquidsStep.Generation)];

	public override void Load()
	{
		On_Lighting.AddLight_int_int_float_float_float += HijackAddLight;
		On_Player.ItemCheck += SoftenPlayerLight;

		LightGlow = Mod.Assets.Request<Texture2D>("Assets/Misc/VFX/LightGlow");
	}

	private static FastNoiseLite GetSurfaceNoise()
	{
		var noise = new FastNoiseLite();
		noise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
		noise.SetFrequency(0.04f);
		return noise;
	}

	private void Polish(GenerationProgress progress, GameConfiguration configuration)
	{
		var noise = new FastNoiseLite();
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		noise.SetFrequency(0.2f);

		var stoneNoise = new FastNoiseLite();
		stoneNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
		stoneNoise.SetFrequency(0.08f);

		HashSet<Point16> structures = [];

		for (int i = 0; i < Width; ++i)
		{
			for (int j = 20; j < Height; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (j > Surface + 20 && tile.WallType == WallID.None || tile.WallType == WallID.SnowWallUnsafe)
				{
					tile.WallType = noise.GetNoise(i, j) > 0 ? WallID.IceUnsafe : WallID.SnowWallUnsafe;
				}

				if (tile.HasTile && tile.TileType == TileID.SnowBlock)
				{
					tile.TileType = noise.GetNoise(i, j + 600) < -0.55f ? TileID.Stone : TileID.SnowBlock;
					float stone = stoneNoise.GetNoise(i, j);

					if (stone > 0.25f)
					{
						tile.TileType = TileID.IceBlock;
					}

					if (WorldGen.genRand.NextBool(300) && j < Surface + 5 && OpenExtensions.GetOpenings(i, j).HasFlag(OpenFlags.Above))
					{
						structures.Add(new Point16(i, j));
					}
				}
			}
		}

		for (int i = 0; i < Width; ++i)
		{
			for (int j = 20; j < Height; ++j)
			{
				WorldGen.TileFrame(i, j);

				if (WorldGen.genRand.NextBool(3) && WorldGen.InWorld(i, j, 2))
				{
					Tile.SmoothSlope(i, j, false);
				}

				if (WorldGen.genRand.NextBool(8))
				{
					WorldGen.PlaceSmallPile(i, j, WorldGen.genRand.Next(36, 48), 0);
				}

				if (j < Surface)
				{
					if (WorldGen.genRand.NextBool(2))
					{
						if (WorldGen.PlaceTile(i, j, TileID.Saplings))
						{
							if (!WorldGen.GrowTree(i, j))
							{
								WorldGen.KillTile(i, j);
							}
						}
					}
				}
				else
				{
					bool farFromSpawn = Vector2.DistanceSquared(new Vector2(i, j), new Vector2(Main.spawnTileX, Main.spawnTileY)) > 30 * 30;

					if (WorldGen.genRand.NextBool(16) && farFromSpawn && Main.tile[i, j - 2].TileType != TileID.BreakableIce)
					{
						WorldGen.PlaceObject(i, j, ModContent.TileType<Icicle>(), true, WorldGen.genRand.Next(3));
					}
					else if (WorldGen.genRand.NextBool(12))
					{
						WorldGen.PlaceUncheckedStalactite(i, j, WorldGen.genRand.NextBool(3), WorldGen.genRand.Next(3), false);
					}
				}
			}
		}

		foreach (Point16 position in structures)
		{
			Point16 pos = position;
			int id = WorldGen.genRand.Next(4);
			var origin = new Vector2(0, 1);

			if (id == 2)
			{
				origin.Y = 0;
				pos = new Point16(pos.X, pos.Y - 2);
			}

			StructureTools.PlaceByOrigin("Assets/Structures/DeerclopsDomain/Surface_" + id, new Point16(pos.X, pos.Y + 2), origin, null, true);
		}
	}

	private void Tunnels(GenerationProgress progress, GameConfiguration configuration)
	{
		FastNoiseLite noise = GetSurfaceNoise();
		Main.spawnTileX = Width / 2;
		Main.spawnTileY = (int)(Height * 0.7f);

		string startPath = "Assets/Structures/DeerclopsDomain/Start_" + WorldGen.genRand.Next(4);
		StructureTools.PlaceByOrigin(startPath, new Point16(Main.spawnTileX, Main.spawnTileY), new(0.5f, 0.6f), null, false);

		int firstTunnelXStart = Main.spawnTileX + WorldGen.genRand.Next(40, 80) * (WorldGen.genRand.NextBool() ? -1 : 1);
		StartTunnel(noise, firstTunnelXStart, out Vector2[] points, out Vector2 last);

		// Second tunnel
		points = Tunnel.GeneratePoints([last, new(MathHelper.Lerp(last.X, Width / 2, 0.3f), last.Y - 80)], 6, 3.5f, 0.5f);
		DigThrough(points, noise, 1);
		AddLanterns(points);
		last = points.Last();
		var chasmPoints = points.Clone() as Vector2[];
		points = Tunnel.CreateEquidistantSet([last, new Vector2(GetOppositeX(last.X), last.Y)], 3.5f);
		last = CreateHorizontalTunnel(noise, points, chasmPoints);

		// Third tunnel
		points = Tunnel.GeneratePoints([last, new(MathHelper.Lerp(last.X, Width / 2, 0.3f), last.Y - 80)], 6, 3.5f, 0.5f);
		DigThrough(points, noise, 1);
		AddLanterns(points);
		last = points.Last();
		chasmPoints = points.Clone() as Vector2[];
		points = Tunnel.CreateEquidistantSet([last, new Vector2(GetOppositeX(last.X), last.Y)], 3.5f);
		CreateHorizontalTunnel(noise, points, chasmPoints);
		last = points.Last();

		// To surface
		points = Tunnel.GeneratePoints([last, new(Width / 2, Surface), new(Width / 2, Surface - 20)], 6, 3.5f, 0.5f);
		DigThrough(points, noise, 1);
		AddLanterns(points, 8);
	}

	private Vector2 CreateHorizontalTunnel(FastNoiseLite noise, Vector2[] points, Vector2[] chasmPoints)
	{
		DigThrough(points, noise, 4);
		PlaceThrower(GetXDirection(points.First().X), points.First());
		Vector2 last = points.Last();
		FindPondLocation(points);
		FindWalls(points, 2, chasmPoints);
		return last;
	}

	private static void FindWalls(Vector2[] points, int wallCount, Vector2[] chasmPoints)
	{
		for (int i = 0; i < wallCount; i++)
		{
			while (true)
			{
				Vector2 pos = Main.rand.Next(points);
				int x = (int)pos.X;
				int y = (int)pos.Y;

				while (!WorldGen.SolidTile(x, y))
				{
					y++;
				}

				int y2 = y - 1;

				while (!WorldGen.SolidTile(x, y2))
				{
					y2--;
				}

				string structure = "Assets/Structures/DeerclopsDomain/Wall_" + WorldGen.genRand.Next(4);
				Point16 size = StructureTools.GetSize(structure);
				int dist = Math.Abs(y - y2);
				var checkingRect = new Rectangle(x, y - (int)(size.Y * 0.5f), size.X, size.Y);
				
				if (chasmPoints.Any(x => checkingRect.Contains(x.ToPoint())))
				{
					continue;
				}

				if (GenVars.structures.CanPlace(new Rectangle(x, y - (int)(size.Y * 0.5f), size.X, size.Y), 10) && dist < size.Y - 4)
				{
					Point16 adjPos = StructureTools.PlaceByOrigin(structure, new Point16(x, (y + y2) / 2), new Vector2(0, 0.5f));
					GenVars.structures.AddProtectedStructure(new Rectangle(adjPos.X, adjPos.Y, size.X, size.Y));
					break;
				}
			}
		}
	}

	private static void FindPondLocation(Vector2[] points)
	{
		int tries = 0;

		while (true)
		{
			tries++;

			if (tries > 20000)
			{
				break;
			}

			int pond = WorldGen.genRand.Next(3);
			Vector2 pos = Main.rand.Next(points);
			int x = (int)pos.X;
			int y = (int)pos.Y;
			
			while (WorldGen.SolidTile(x, y))
			{
				y++;
			}

			string structure = "Assets/Structures/DeerclopsDomain/Pond_" + pond;
			Point16 size = StructureTools.GetSize(structure);
			int y2 = (int)pos.Y;

			while (WorldGen.SolidTile(x + size.X, y2))
			{
				y2++;
			}

			if (Math.Abs(y2 - y) < 3 && GenVars.structures.CanPlace(new Rectangle(x, y, size.X, size.Y)))
			{
				StructureTools.PlaceByOrigin(structure, new Point16(x, y), new Vector2(0));
				GenVars.structures.AddProtectedStructure(new Rectangle(x, y, size.X, size.Y));
				break;
			}
			else
			{
				y2 = (int)pos.Y;

				while (WorldGen.SolidTile(x - size.X, y2))
				{
					y2++;
				}

				if (Math.Abs(y2 - y) < 3 && GenVars.structures.CanPlace(new Rectangle(x - size.X, y, size.X, size.Y)))
				{
					StructureTools.PlaceByOrigin(structure, new Point16(x, y), new Vector2(1, 0));
					GenVars.structures.AddProtectedStructure(new Rectangle(x - size.X, y, size.X, size.Y));
					break;
				}
			}
		}
	}

	private void StartTunnel(FastNoiseLite noise, int firstTunnelXStart, out Vector2[] points, out Vector2 last)
	{
		points = Tunnel.GeneratePoints([new(Main.spawnTileX, Main.spawnTileY), new(firstTunnelXStart, Main.spawnTileY - 60)], 6, 4);
		DigThrough(points, noise, 1);
		AddLanterns(points);
		last = points.Last();
		var chasmPoints = points.Clone() as Vector2[];
		points = Tunnel.CreateEquidistantSet([last, new Vector2(GetOppositeX(last.X), last.Y)], 4);
		last = CreateHorizontalTunnel(noise, points, chasmPoints);
	}

	private static void PlaceThrower(int dir, Vector2 position)
	{
		var pos = position.ToPoint();

		while (!CanPlaceThrower(pos))
		{
			pos.X += dir;
		}

		if (pos.X < 10 || pos.X > Main.maxTilesX - 10)
		{
			return;
		}

		int startX = dir == -1 ? pos.X : pos.X - 8;
		int endX = dir == -1 ? pos.X + 8 : pos.X + 2;

		for (int i = startX; i < endX; ++i)
		{
			for (int j = pos.Y - 1; j < pos.Y + 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (j <= pos.Y)
				{
					tile.HasTile = false;
				}
			}
		}

		WorldGen.PlaceObject(pos.X, pos.Y, ModContent.TileType<PolarIceThrower>(), true, direction: -dir);
		ModContent.GetInstance<PolarIceThrower.ThrowerTE>().Place(pos.X, pos.Y - 1);
	}

	public static bool CanPlaceThrower(Point position)
	{
		if (position.X < 10 || position.X > Main.maxTilesX - 10)
		{
			return true;
		}

		for (int i = position.X; i < position.X + 2; ++i)
		{
			for (int j = position.Y - 1; j < position.Y + 2; ++j)
			{
				if (!WorldGen.SolidTile(i, j))
				{
					return false;
				}
			}
		}

		return true;
	}

	private static void AddLanterns(Vector2[] points, int count = 4)
	{
		for (int i = 0; i < count; ++i)
		{
			var pos = points[i * (points.Length / (count + 1))].ToPoint();

			while (!WorldGen.SolidTile(pos.X, pos.Y))
			{
				pos.Y--;

				if (!WorldGen.InWorld(pos.X, pos.Y, 6))
				{
					break;
				}
			}

			if (!WorldGen.InWorld(pos.X, pos.Y, 6))
			{
				continue;
			}

			WorldGen.PlaceObject(pos.X, pos.Y + 2, (ushort)ModContent.TileType<PolarIceLantern>(), true);
			pos.Y++;

			while (!WorldGen.SolidTile(pos.X, pos.Y))
			{
				pos.Y++;

				if (!WorldGen.InWorld(pos.X, pos.Y, 6))
				{
					break;
				}
			}

			if (!WorldGen.InWorld(pos.X, pos.Y, 6))
			{
				continue;
			}

			WorldGen.PlaceObject(pos.X, pos.Y - 2, (ushort)ModContent.TileType<PolarIceLamp>(), true);
		}
	}

	private float GetOppositeX(float x)
	{
		return x > Width / 2 ? MathF.Max(x - 400, 80) : MathF.Min(x + 400, Main.maxTilesX - 80);
	}

	private int GetXDirection(float x)
	{
		return x > Width / 2 ? 1 : -1;
	}

	private static void DigThrough(Vector2[] points, FastNoiseLite noise, float size)
	{
		foreach (Vector2 item in points)
		{
			Vector2 pos = item;
			pos += Main.rand.NextVector2Circular(2, 6);
			float mul = 1f + MathF.Abs(noise.GetNoise(pos.X, pos.Y)) * 1.2f * size;
			Digging.CircleOpening(pos, 5 * mul);
			Digging.CircleOpening(pos, WorldGen.genRand.Next(3, 7) * mul);

			if (WorldGen.genRand.NextBool(3, 5))
			{
				WorldGen.digTunnel(pos.X, pos.Y, 0, 0, 5, (int)(WorldGen.genRand.NextFloat(1, 8) * mul));
			}
		}
	}

	private void SoftenPlayerLight(On_Player.orig_ItemCheck orig, Player self)
	{
		LightMultiplier = 0.15f;
		orig(self);
		LightMultiplier = 0;
	}

	private void HijackAddLight(On_Lighting.orig_AddLight_int_int_float_float_float orig, int i, int j, float r, float g, float b)
	{
		if (SubworldSystem.Current is DeerclopsDomain && j > Surface + 10)
		{
			(r, g, b) = (r * LightMultiplier, g * LightMultiplier, b * LightMultiplier);
		}

		orig(i, j, r, g, b);
	}

	public override bool GetLight(Tile tile, int x, int y, ref FastRandom rand, ref Vector3 color)
	{
		if (y > Surface)
		{
			float mul = 0;

			if (y < Surface + 10)
			{
				mul = (y - Surface) / 10f;
			}

			color *= mul;
			return true;
		}

		return false;
	}

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;
	}

	public override void Update()
	{
		Liquid.UpdateLiquid();

		TileEntity.UpdateStart();
		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();

		Main.moonPhase = (int)MoonPhase.Full;
		bool playersOnSurface = Main.CurrentFrameFlags.ActivePlayersCount > 0;

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.Center.Y > Surface * 16)
			{
				playersOnSurface = false;
				break;
			}
		}

		if (!BossSpawned && playersOnSurface)
		{
			BossSpawned = true;
			NPC.SpawnOnPlayer(0, NPCID.Deerclops);

			Main.spawnTileX = Width / 2 + Main.rand.Next(20, 25) + (Main.rand.NextBool() ? -1 : 1);
			Main.spawnTileY = Surface - 10;

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.Deerclops) && !ReadyToExit)
		{
			Vector2 pos = new Vector2(Width / 2, Height / 4 - 8) * 16;
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.Deerclops);
			ReadyToExit = true;
		}
	}
}
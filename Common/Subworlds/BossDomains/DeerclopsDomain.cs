using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.Systems.DisableBuilding;
using Terraria.Enums;
using PathOfTerraria.Common.World.Generation;
using Terraria.DataStructures;
using PathOfTerraria.Common.Subworlds.Passes;
using SubworldLibrary;
using Terraria.Utilities;
using System.Linq;
using PathOfTerraria.Content.Tiles.BossDomain;
using ReLogic.Content;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class DeerclopsDomain : BossDomainSubworld
{
	internal static Asset<Texture2D> LightGlow = null;

	public static int Surface => 200;

	public override int Width => 800;
	public override int Height => 800;
	public override int[] WhitelistedCutTiles => [TileID.BreakableIce];
	public override int[] WhitelistedMiningTiles => [TileID.BreakableIce];

	internal static float LightMultiplier = 0;

	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), 
		new FlatWorldPass(Surface, true, GetSurfaceNoise(), TileID.SnowBlock, WallID.SnowWallUnsafe), 
		new PassLegacy("Tunnels", Tunnels),
		new PassLegacy("Polish", Polish)];

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
		TileID.Sets.CanBeClearedDuringGeneration[TileID.SnowBlock] = true;
		TileID.Sets.CanBeClearedDuringOreRunner[TileID.SnowBlock] = true;

		for (int i = 0; i < Width; ++i)
		{
			for (int j = Surface + 20; j < Height; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.TileType == TileID.SnowBlock)
				{
					tile.TileType = TileID.Chlorophyte;
				}
			}
		}

		var noise = new FastNoiseLite();
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		noise.SetFrequency(0.2f);

		var stoneNoise = new FastNoiseLite();
		stoneNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
		stoneNoise.SetFrequency(0.08f);

		for (int i = 0; i < Width; ++i)
		{
			for (int j = Surface + 20; j < Height; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.WallType == WallID.None || tile.WallType == WallID.SnowWallUnsafe)
				{
					tile.WallType = noise.GetNoise(i, j) > 0 ? WallID.IceUnsafe : WallID.SnowWallUnsafe;
				}

				if (tile.HasTile && tile.TileType == TileID.Chlorophyte)
				{
					tile.TileType = noise.GetNoise(i, j + 600) < -0.55f ? TileID.Stone : TileID.Chlorophyte;
					float stone = stoneNoise.GetNoise(i, j);

					if (stone > 0.25f)
					{
						tile.TileType = TileID.IceBlock;
					}
				}
			}
		}

		for (int i = 0; i < Width; ++i)
		{
			for (int j = Surface + 20; j < Height; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.TileType == TileID.Chlorophyte)
				{
					tile.TileType = TileID.SnowBlock;
				}
			}
		}

		TileID.Sets.CanBeClearedDuringGeneration[TileID.SnowBlock] = false;
		TileID.Sets.CanBeClearedDuringOreRunner[TileID.SnowBlock] = false;
	}

	private void Tunnels(GenerationProgress progress, GameConfiguration configuration)
	{
		FastNoiseLite noise = GetSurfaceNoise();
		Main.spawnTileX = Width / 2;
		Main.spawnTileY = (int)(Height * 0.7f);

		StructureTools.PlaceByOrigin("Assets/Structures/DeerclopsDomain/Start_0", new Point16(Main.spawnTileX, Main.spawnTileY), new(0.5f), null, false);

		int firstTunnelXStart = Main.spawnTileX + WorldGen.genRand.Next(40, 80) * (WorldGen.genRand.NextBool() ? -1 : 1);
		StartTunnel(noise, firstTunnelXStart, out Vector2[] points, out Vector2 last);

		// Second tunnel
		points = Tunnel.GeneratePoints([last, new(MathHelper.Lerp(last.X, Width / 2, 0.3f), last.Y - 80)], 6, 4, 0.5f);
		DigThrough(points, noise, 1);
		AddLanterns(points);
		last = points.Last();
		points = Tunnel.CreateEquidistantSet([last, new Vector2(GetOppositeX(last.X), last.Y)], 4);
		last = CreateHorizontalTunnel(noise, points);

		// Third tunnel
		points = Tunnel.GeneratePoints([last, new(MathHelper.Lerp(last.X, Width / 2, 0.3f), last.Y - 80)], 6, 4, 0.5f);
		DigThrough(points, noise, 1);
		AddLanterns(points);
		last = points.Last();
		points = Tunnel.CreateEquidistantSet([last, new Vector2(GetOppositeX(last.X), last.Y)], 4);
		CreateHorizontalTunnel(noise, points);
		last = points.Last();

		// To surface
		points = Tunnel.GeneratePoints([last, new(Width / 2, Surface), new(Width / 2, Surface - 20)], 6, 4, 0.5f);
		DigThrough(points, noise, 1);
		AddLanterns(points);
	}

	private Vector2 CreateHorizontalTunnel(FastNoiseLite noise, Vector2[] points)
	{
		Vector2 last;
		DigThrough(points, noise, 4);
		PlaceThrower(GetXDirection(points.First().X), points.First());
		last = points.Last();
		FindPondLocation(points);
		FindWalls(points, 2);
		return last;
	}

	private static void FindWalls(Vector2[] points, int v)
	{
		for (int i = 0; i < v; i++)
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

				string structure = "Assets/Structures/DeerclopsDomain/Wall_" + WorldGen.genRand.Next(2);
				Point16 size = StructureTools.GetSize(structure);
				int dist = Math.Abs(y - y2);

				if (GenVars.structures.CanPlace(new Rectangle(x, y - (int)(size.Y * 0.9f), size.X, size.Y), 10) && dist < size.Y - 4)
				{
					Point16 adjPos = StructureTools.PlaceByOrigin(structure, new Point16(x, y), new Vector2(0, 0.9f));
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

			int pond = WorldGen.genRand.Next(2);
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
		points = Tunnel.CreateEquidistantSet([last, new Vector2(GetOppositeX(last.X), last.Y)], 4);
		last = CreateHorizontalTunnel(noise, points);
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

	private static void AddLanterns(Vector2[] points)
	{
		for (int i = 0; i < 4; ++i)
		{
			var pos = points[i * (points.Length / 5)].ToPoint();

			while (!WorldGen.SolidTile(pos.X, pos.Y))
			{
				pos.Y--;
			}

			WorldGen.PlaceObject(pos.X, pos.Y + 2, (ushort)ModContent.TileType<PolarIceLantern>(), true);
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
		TileEntity.UpdateStart();
		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();

		Main.dayTime = true;
		Main.time = Main.dayLength / 2;
		Main.moonPhase = (int)MoonPhase.Full;

		foreach (Player player in Main.ActivePlayers)
		{
			player.GetModPlayer<StopBuildingPlayer>().ConstantStopBuilding = true;
		}

		if (!BossSpawned && NPC.AnyNPCs(NPCID.QueenBee))
		{
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.QueenBee) && !ReadyToExit)
		{
			Vector2 pos = new Vector2(Width / 2, Height / 4 - 8) * 16;
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.QueenBee);
			ReadyToExit = true;
		}
	}
}
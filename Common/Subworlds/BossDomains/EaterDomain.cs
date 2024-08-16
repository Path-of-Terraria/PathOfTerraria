using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.Systems.DisableBuilding;
using Terraria.DataStructures;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.GameContent;
using ReLogic.Graphics;
using PathOfTerraria.Content.Tiles.BossDomain;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class EaterDomain : BossDomainSubworld
{
	internal static bool SpawningEoW = false;

	public override int Width => 800;
	public override int Height => 1000;

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;
	public List<Vector2> SlimePositions = [];

	private HashSet<Point16> DemonitePositions = [];

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenTerrain),
		new PassLegacy("Arena", SpawnArena),
		new PassLegacy("Grasses", PlaceGrassAndDecor)];

	private void PlaceGrassAndDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = "Populating world...";

		Dictionary<Point16, OpenFlags> tiles = [];

		for (int i = 1; i < Main.maxTilesX - 1; ++i)
		{
			for (int j = 80; j < Main.maxTilesY - 60; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile || tiles.ContainsKey(new Point16(i, j)))
				{
					continue;
				}

				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

				if (flags == OpenFlags.None)
				{
					continue;
				}

				tiles.Add(new Point16(i, j), flags);
			}
		}
		
		foreach (KeyValuePair<Point16, OpenFlags> item in tiles)
		{
			Tile tile = Main.tile[item.Key];

			if (tile.TileType == TileID.Dirt)
			{
				tile.TileType = TileID.CorruptGrass;
			}

			PlaceDecor(item.Key, item.Value);
		}

		foreach (Point16 positions in DemonitePositions)
		{
			ushort type = WorldGen.genRand.NextBool(5) ? TileID.Gold : TileID.Demonite;
			WorldGen.TileRunner(positions.X, positions.Y, WorldGen.genRand.Next(6, 16), WorldGen.genRand.Next(4, 20), type);
		}
	}

	private static void PlaceDecor(Point16 position, OpenFlags flags)
	{
		if (WorldGen.genRand.NextBool(8))
		{
			Tile.SmoothSlope(position.X, position.Y, true);
			return;
		}

		Tile tile = Main.tile[position];

		if (flags.HasFlag(OpenFlags.Above))
		{
			if (tile.TileType == TileID.CorruptGrass)
			{
				if (!WorldGen.genRand.NextBool(3))
				{
					WorldGen.PlaceTile(position.X, position.Y - 1, TileID.CorruptPlants, true);
				}
				else if (WorldGen.genRand.NextBool(6))
				{
					WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Saplings);

					if (!WorldGen.GrowTree(position.X, position.Y - 1))
					{
						WorldGen.KillTile(position.X, position.Y - 1);
					}
				}
			}
			else if (tile.TileType == TileID.Ebonstone)
			{
				if (WorldGen.genRand.NextBool(4))
				{
					WorldGen.PlaceSmallPile(position.X, position.Y - 1, WorldGen.genRand.Next(12, 28), 0);
				}
				else if (WorldGen.genRand.NextBool(3))
				{
					WorldGen.PlaceUncheckedStalactite(position.X, position.Y - 1, WorldGen.genRand.NextBool(4), WorldGen.genRand.Next(15, 19), false);
				}
			}
		}
		else if (flags.HasFlag(OpenFlags.Below))
		{
			if (tile.TileType == TileID.Ebonstone)
			{
				if (WorldGen.genRand.NextBool(3))
				{
					WorldGen.PlaceUncheckedStalactite(position.X, position.Y + 1, WorldGen.genRand.NextBool(4), WorldGen.genRand.Next(15, 19), false);
				}
			}
		}
	}

	private void SpawnArena(GenerationProgress progress, GameConfiguration configuration)
	{
		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Assets/Structures/EaterArena", Mod, ref size);
		var position = new Point16(400 - size.X / 2, Height - 150 - size.Y / 2);
		StructureHelper.Generator.GenerateStructure("Assets/Structures/EaterArena", position, Mod);

		Arena = new Rectangle(position.X * 16, position.Y * 16, size.X * 16, size.Y * 16);
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = WorldGen.genRand.NextBool() ? 80 : Main.maxTilesX - 80;
		Main.spawnTileY = 110;
		Main.worldSurface = 230;
		Main.rockLayer = 299;

		float baseY = 120;

		FastNoiseLite noise = GetGenNoise();
		DemonitePositions = [];
		progress.Message = "Generating terrain";

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			float noiseOffset = noise.GetNoise(x, 0) * 3;
			float useY = baseY + noiseOffset;
			int difference = Math.Abs(x - 400);

			if (difference < 200)
			{
				useY += (200 - difference) / 4;
			}

			for (int y = (int)useY; y < Main.maxTilesY; ++y)
			{
				WorldGen.PlaceTile(x, y, y > 400 + noiseOffset ? TileID.Ebonstone : TileID.Dirt);

				if (y > useY + 4)
				{
					WorldGen.PlaceWall(x, y, WallID.EbonstoneUnsafe, true);
				}

				if (y > 400 + noiseOffset && WorldGen.genRand.NextBool(1600))
				{
					DemonitePositions.Add(new Point16(x, y));
				}
			}

			progress.Value = (float)x / Main.maxTilesX;
		}

		progress.Message = "Digging tunnels";

		// Chasm one
		List<Vector2> breakthroughs = [];
		DigChasm(noise, Tunnel.GeneratePoints(GenerateWindingTunnel(400, baseY, 400, baseY + 200), 26, 6), null);

		// Tunnel one
		Vector2[] horizontalPoints = Tunnel.GeneratePoints(GenerateHorizontalTunnel(100, baseY + 200, 700, baseY + 200), 20, 10, 0.3f);
		DigChasm(noise, horizontalPoints, (120, 680, 20), 2.4f, true);
		GetRandomPoint(breakthroughs, horizontalPoints);

		// Chasm two
		DigChasm(noise, Tunnel.GeneratePoints(GenerateWindingTunnel((int)breakthroughs[0].X, (int)breakthroughs[0].Y, 400, (int)breakthroughs[0].Y + 200), 26, 10), null);

		// Tunnel two
		horizontalPoints = Tunnel.GeneratePoints(GenerateHorizontalTunnel(100, (int)breakthroughs[0].Y + 200, 700, (int)breakthroughs[0].Y + 200), 15, 10, 0.3f);
		DigChasm(noise, horizontalPoints, (120, 680, 20), 2.4f, true);
		breakthroughs.Add(WorldGen.genRand.Next(horizontalPoints));

		// Last chasm
		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Assets/Structures/EaterArena", Mod, ref size);
		DigChasm(noise, Tunnel.GeneratePoints(GenerateWindingTunnel((int)breakthroughs[1].X, (int)breakthroughs[1].Y, 400, Height - 250, 0.2f), 12, 10), null);
		DigChasm(noise, Tunnel.GeneratePoints(GenerateWindingTunnel(400, Height - 260, 400, Height - 120, 0.1f), 12, 10), null);

		// Opening before the arena
		WorldGen.digTunnel(400, Height - 210, 0, 0, 20, 20);

		foreach (Vector2 item in breakthroughs)
		{
			WorldGen.TileRunner((int)item.X, (int)item.Y, 26, WorldGen.genRand.Next(4, 20), ModContent.TileType<WeakMalaise>(), true);
		}
	}

	private static void GetRandomPoint(List<Vector2> breakthroughs, Vector2[] horizontalPoints)
	{
		Vector2 position = WorldGen.genRand.Next(horizontalPoints);

		while (Math.Abs(position.X - 400) < 100)
		{
			position = WorldGen.genRand.Next(horizontalPoints);
		}

		breakthroughs.Add(position);
	}

	private Vector2[] GenerateHorizontalTunnel(int x, float y, int targetX, float targetY)
	{
		const int MaxSteps = 8;

		List<Vector2> positions = [new Vector2(x, y)];

		for (int i = 1; i < MaxSteps - 1; ++i)
		{
			var lerp = Vector2.Lerp(new Vector2(x, y), new Vector2(targetX, targetY), i / (MaxSteps - 1f));
			lerp += new Vector2(0, WorldGen.genRand.Next(-2, 2));

			positions.Add(lerp);
		}

		positions.Add(new Vector2(targetX, targetY));
		return [.. positions];
	}

	private static void DigChasm(FastNoiseLite noise, Vector2[] positions, (int baseX, int endX, int fadeAway)? smoothInOut, float sizeMul = 1.2f, bool digTunnel = true)
	{
		foreach (Vector2 item in positions)
		{
			float mul = 1f + MathF.Abs(noise.GetNoise(item.X, item.Y)) * sizeMul;

			if (smoothInOut.HasValue)
			{
				if (item.X < smoothInOut.Value.baseX)
				{
					mul *= (20 - MathF.Min(smoothInOut.Value.baseX - item.X, 20)) / smoothInOut.Value.fadeAway;
				}
				else if (item.X > smoothInOut.Value.endX)
				{
					mul *= (20 - MathF.Min(item.X - smoothInOut.Value.endX, 20)) / smoothInOut.Value.fadeAway;
				}
			}

			Digging.CircleOpening(item, 5 * mul);
			Digging.CircleOpening(item, WorldGen.genRand.Next(3, 7) * mul);

			if (WorldGen.genRand.NextBool(8))
			{
				Digging.WallCircleOpening(item, WorldGen.genRand.Next(4, 7));
			}

			if (digTunnel && WorldGen.genRand.NextBool(3, 5))
			{
				WorldGen.digTunnel(item.X, item.Y, 0, 0, 5, (int)(WorldGen.genRand.NextFloat(1, 8) * mul));
			}

			WorldGen.TileRunner((int)item.X, (int)item.Y, 120, 8, TileID.Ebonstone, false, 0, 0, false, true);
		}
	}

	private Vector2[] GenerateWindingTunnel(int x, float y, int targetX, float targetY, float windingMultiplier = 1f)
	{
		const int MaxSteps = 3;

		List<Vector2> positions = [new Vector2(x, y)];

		for (int i = 0; i < MaxSteps - 1; ++i)
		{
			var lerp = Vector2.Lerp(new Vector2(x, y), new Vector2(targetX, targetY), i / (MaxSteps - 1f));
			lerp += new Vector2((int)(WorldGen.genRand.Next(-100, 100) * windingMultiplier), (int)(WorldGen.genRand.Next(-20, 20) * windingMultiplier));
			positions.Add(lerp);
		}

		positions.Add(new Vector2(targetX, targetY));
		return [.. positions];
	}

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.008f);
		return noise;
	}

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;
		SlimePositions.Clear();

		Main.dayTime = true;
		Main.time = Main.dayLength - 1800;
	}

	public override void Update()
	{
		Main.dayTime = true;
		Main.time = Main.dayLength - 1800;

		bool allInArena = true;

		foreach (Player player in Main.ActivePlayers)
		{
			player.GetModPlayer<StopBuildingPlayer>().ConstantStopBuilding = true;

			if (allInArena && !Arena.Intersects(player.Hitbox))
			{
				allInArena = false;
			}
		}

		if (!BossSpawned && allInArena)
		{
			for (int i = 0; i < 20; ++i)
			{
				WorldGen.PlaceTile(Arena.X / 16 + i + 4, Arena.Y / 16 - 3, TileID.FleshBlock, true, true);
			}

			SpawningEoW = true;
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X + 1400, Arena.Center.Y - 0, NPCID.EaterofWorldsHead, 1);
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X - 1400, Arena.Center.Y - 0, NPCID.EaterofWorldsHead, 1);
			SpawningEoW = false;
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.EaterofWorldsHead) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(-130, -300);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.EaterofWorldsHead);
			ReadyToExit = true;
		}
	}
}

/// <summary>
/// Used solely to reduce the number of segments per Eater from 67/72 to 30/34 in order to account for there being two Eaters in <see cref="EaterDomain"/>.
/// </summary>
public class EaterEdit : ILoadable
{
	public void Load(Mod mod)
	{
		IL_NPC.AI_006_Worms += HijackSpawnBody;
	}

	private void HijackSpawnBody(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchCall<NPC>(nameof(NPC.GetEaterOfWorldsSegmentsCount))))
		{
			return;
		}

		if (!c.TryGotoNext(x => x.MatchCall<NPC>(nameof(NPC.NewNPC))))
		{
			return;
		}

		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate((NPC npc) =>
		{
			if (EaterDomain.SpawningEoW)
			{
				npc.ai[2] = Main.expertMode ? 34 : 30;
			}
		});
	}

	public void Unload() { }
}
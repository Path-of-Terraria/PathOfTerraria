using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using SubworldLibrary;
using Terraria.Enums;
using Terraria.Localization;
using PathOfTerraria.Common.World;
using System.Linq;
using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.Audio;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Common.Subworlds.BossDomains.BoCDomain;
using PathOfTerraria.Content.NPCs.Town;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class BrainDomain : BossDomainSubworld
{
	public override int Width => 800;
	public override int Height => 1300;
	public override int[] WhitelistedCutTiles => [TileID.Pots, TileID.CrimsonThorns];
	public override int DropItemLevel => 20;
	public override (int time, bool isDay) ForceTime => ((int)Main.nightLength / 2, false);

	public Rectangle Arena = Rectangle.Empty;
	public Vector2 ProjectilePosition = Vector2.Zero;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenSurface),
		new PassLegacy("Arena", SpawnArena),
		new PassLegacy("Decor", SpawnDecor)];

	public override void CopyMainWorldData()
	{
		SubworldSystem.CopyWorldData("hasLloyd", ModContent.GetInstance<BoCDomainSystem>().HasLloyd);
	}

	public override void ReadCopiedMainWorldData()
	{
		ModContent.GetInstance<BoCDomainSystem>().HasLloyd = SubworldSystem.ReadCopiedWorldData<bool>("hasLloyd");
	}

	private void SpawnDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		Dictionary<Point16, OpenFlags> tiles = [];

		for (int i = 6; i < Main.maxTilesX - 6; ++i)
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

			progress.Value = (float)i / Main.maxTilesX;
		}

		foreach ((Point16 position, OpenFlags flags) in tiles)
		{
			Tile tile = Main.tile[position];

			if (tile.TileType == TileID.Crimstone && flags.HasFlag(OpenFlags.Below))
			{
				for (int i = position.Y; i > position.Y - 4; --i)
				{
					if (WorldGen.SolidOrSlopedTile(position.X, i))
					{
						WorldGen.PlaceTile(position.X, i, TileID.Dirt, true, true);
					}
				}
			}
		}

		foreach ((Point16 position, OpenFlags flags) in tiles)
		{
			Tile tile = Main.tile[position];

			if (tile.TileType == TileID.Dirt)
			{
				if (WorldGen.SolidOrSlopedTile(position.X, position.Y))
				{
					WorldGen.PlaceTile(position.X, position.Y, TileID.CrimsonGrass, true, true);

					PlaceGrassDecor(position, flags);
				}
			}
		}

		if (ModContent.GetInstance<BoCDomainSystem>().HasLloyd)
		{
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<LloydNPC>());
		}
	}

	private static void PlaceGrassDecor(Point16 position, OpenFlags flags)
	{
		if (!Main.tile[position.X, position.Y - 1].HasTile)
		{
			if (WorldGen.genRand.NextBool(12))
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Saplings);

				if (!WorldGen.GrowTree(position.X, position.Y - 1))
				{
					WorldGen.KillTile(position.X, position.Y - 1);
				}
			}
			else if (WorldGen.genRand.NextBool(40) && Math.Abs(position.X - Main.spawnTileX) > 40)
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, ModContent.TileType<Pustule>(), style: WorldGen.genRand.Next(2));
			}
			else if (WorldGen.genRand.NextBool(30))
			{
				WorldGen.PlacePot(position.X, position.Y - 1, TileID.Pots, WorldGen.genRand.Next(22, 25));
			}
			else
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, TileID.CrimsonPlants, true);
			}
		}

		if (flags.HasFlag(OpenFlags.Below))
		{
			int length = WorldGen.genRand.Next(2, 11);

			for (int i = 1; i < length; ++i)
			{
				if (Main.tile[position.X, position.Y + i].HasTile)
				{
					break;
				}

				WorldGen.PlaceTile(position.X, position.Y + i, TileID.CrimsonVines, true);
			}
		}
	}

	private void SpawnArena(GenerationProgress progress, GameConfiguration configuration)
	{
		var dims = new Point16();
		StructureHelper.Generator.GetDimensions("Assets/Structures/BrainArena", Mod, ref dims);
		var pos = new Point16(400 - dims.X / 2, 200 - dims.Y / 2);
		StructureHelper.Generator.GenerateStructure("Assets/Structures/BrainArena", pos, Mod);
		Arena = new Rectangle(pos.X * 16, pos.Y * 16, dims.X * 16, dims.Y * 16);
	}

	private void GenSurface(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = WorldGen.genRand.NextBool() ? 80 : Width - 80;
		Main.spawnTileY = Height - 210;
		Main.worldSurface = Height - 180;
		Main.rockLayer = Height - 190;
		float baseY = Height - 200;
		FastNoiseLite noise = GetGenNoise();

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			float useY = baseY + noise.GetNoise(x, 0) * 4;

			for (int y = (int)useY; y < Main.maxTilesY; ++y)
			{
				Tile tile = Main.tile[x, y];
				tile.HasTile = true;
				tile.TileType = y > useY + noise.GetNoise(x, 300) * 8 + 20 ? TileID.Crimtane : TileID.Dirt;
			}

			progress.Value = (float)x / Main.maxTilesX;
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Steps");
		GenerateSteps(progress);
	}

	private void GenerateSteps(GenerationProgress progress)
	{
		int lineCount = 4;
		int x = Main.spawnTileX >= 400 ? 420 : 380;
		int y = Height - 210;
		Queue<(Point16, Point16)> lines = [];

		for (int i = 0; i < lineCount; ++i)
		{
			if (i != 0)
			{
				if (x > 400)
				{
					x -= 110;
				}
				else
				{
					x += 110;
				}

				y -= 40;
			}

			Point16 begin = new(x, y);
			int nextX = x;
			int nextY = y;

			if (x > 400)
			{
				nextX = WorldGen.genRand.Next(80, 300);
			}
			else
			{
				nextX = WorldGen.genRand.Next(500, 720);
			}

			nextY -= WorldGen.genRand.Next(120, 160);
			lines.Enqueue((begin, new Point16(nextX, nextY)));
			x = nextX;
			y = nextY;

			progress.Value = i / (lineCount - 1f);
		}

		progress.Value = 0;

		foreach ((Point16 start, Point16 end) in lines) 
		{
			Vector2[] basePoints = [start.ToVector2(), Vector2.Lerp(start.ToVector2(), end.ToVector2(), 0.5f), end.ToVector2()];
			Vector2[] points = Tunnel.CreateEquidistantSet(basePoints, 46);
			float startSize = WorldGen.genRand.NextFloat(80, 110);
			float endSize = WorldGen.genRand.NextFloat(80, 110);
			float startAngle = basePoints[0].AngleTo(basePoints[2]) - MathHelper.PiOver2;
			float endAngle = startAngle + WorldGen.genRand.NextFloat(MathHelper.PiOver2) * (start.X < 400 ? -1 : 1);
			float maxValue = 1 / (float)lines.Count;

			for (int i = 0; i < points.Length; i++)
			{
				float factor = i / (points.Length - 1f);
				GenOval(points[i], MathHelper.Lerp(startSize, endSize, factor), MathHelper.Lerp(startAngle, endAngle, factor), false);
				progress.Value += 1 / (float)points.Length * maxValue;
			}
		}

		foreach ((Point16 start, Point16 end) in lines)
		{
			Vector2[] basePoints = [start.ToVector2(), Vector2.Lerp(start.ToVector2(), end.ToVector2(), 0.5f), end.ToVector2()];
			Vector2[] points = Tunnel.CreateEquidistantSet(basePoints, 46);
			float startSize = WorldGen.genRand.NextFloat(80, 110);
			float endSize = WorldGen.genRand.NextFloat(80, 110);
			float startAngle = basePoints[0].AngleTo(basePoints[2]) - MathHelper.PiOver2;
			float endAngle = startAngle + WorldGen.genRand.NextFloat(MathHelper.PiOver2) * (start.X < 400 ? -1 : 1);
			float maxValue = 1 / (float)lines.Count;
			Vector2 direction = basePoints[0].DirectionTo(basePoints[2]).RotatedByRandom(MathHelper.PiOver4) * WorldGen.genRand.NextFloat(-30, 30);

			for (int i = 0; i < points.Length; i++)
			{
				float factor = i / (points.Length - 1f);
				GenOval(points[i] + direction, MathHelper.Lerp(startSize, endSize, factor), MathHelper.Lerp(startAngle, endAngle, factor), true);
				progress.Value += 1 / (float)points.Length * maxValue;
			}
		}

		PlaceCrimtane();

		Point16 lastEnd = lines.Last().Item2;
		var pos = new Vector2(lastEnd.X + (lastEnd.X > 400 ? 30 : -30), lastEnd.Y);
		Point16 size = StructureTools.GetSize("Assets/Structures/BrainDomain/PortalIsland", Mod);

		while (Collision.SolidCollision(pos, size.X * 16, size.Y * 16))
		{
			pos.X += pos.X > 400 ? 64 : -64;
		}

		StructureTools.PlaceByOrigin("Assets/Structures/BrainDomain/PortalIsland", pos.ToPoint16(), new(0.5f));

		ProjectilePosition = pos;
	}

	private void PlaceCrimtane()
	{
		for (int i = 0; i < 120; ++i)
		{
			int x = WorldGen.genRand.Next(100, Width - 100);
			int y = WorldGen.genRand.Next(300, Height - 200);

			if (Main.tile[x, y].TileType != TileID.Crimstone)
			{
				i--;
				continue;
			}

			WorldGen.TileRunner(x, y, WorldGen.genRand.Next(6, 16), WorldGen.genRand.Next(4, 20), TileID.Crimtane);
		}
	}

	private static void GenOval(Vector2 origin, float size, float angle, bool isWall)
	{
		var otherEnd = (origin + new Vector2(size, size / 2)).ToPoint16();
		List<Point16> results = [];
		float ySize = size / WorldGen.genRand.NextFloat(2, 3);
		Ellipse.Fill(!isWall ? (x, y) => FastPlaceTile(x, y, TileID.Crimstone) : (x, y) => FastPlaceWall(x, y, WallID.CrimstoneUnsafe), 
			origin.ToPoint16(), size, ySize, angle - MathHelper.PiOver2, ref results, (x, y) => GetGenNoise().GetNoise(x, y) * 10);
	}

	public static void FastPlaceTile(int x, int y, int type)
	{
		Tile tile = Main.tile[x, y];
		tile.TileType = (ushort)type;
		tile.HasTile = true;
	}

	public static void FastPlaceWall(int x, int y, int type)
	{
		Tile tile = Main.tile[x, y];
		tile.WallType = (ushort)type;
		tile.HasTile = true;
	}

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.01f);
		return noise;
	}

	private static FastNoiseLite GetLightNoise()
	{
		var noise = new FastNoiseLite(23908);
		noise.SetFrequency(0.04f);
		return noise;
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ReadyToExit = false;
	}

	public override bool GetLight(Tile tile, int x, int y, ref FastRandom rand, ref Vector3 color)
	{
		if (!Main.tile[x, y].HasTile)
		{
			float factor = ModContent.GetInstance<BoCDomainSystem>().DomainAtmosphere;
			color = Vector3.Max(color, Vector3.Lerp(color, Color.PaleVioletRed.ToVector3(), factor));
		}

		if (Main.tile[x, y].WallType != WallID.None && !WallID.Sets.Transparent[Main.tile[x, y].WallType])
		{
			color *= MathF.Max(0, MathF.Pow(GetLightNoise().GetNoise(x, y), 4)) * 0.7f;
		}

		return true;
	}

	public override void Update()
	{
		bool hasProj = false;

		foreach (Projectile proj in Main.ActiveProjectiles)
		{
			if (proj.type == ModContent.ProjectileType<Teleportal>())
			{
				hasProj = true;
			}
		}

		if (!hasProj && Main.CurrentFrameFlags.ActivePlayersCount > 0)
		{
			int type = ModContent.ProjectileType<Teleportal>();
			Vector2 position = ProjectilePosition.ToWorldCoordinates() - new Vector2(10, 80);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), position, Vector2.Zero, type, 0, 0, -1, 400 * 16, 200 * 16);
		}

		Main.moonPhase = (int)MoonPhase.Full;

		bool allInArena = Main.CurrentFrameFlags.ActivePlayersCount > 0;

		foreach (Player player in Main.ActivePlayers)
		{
			if (allInArena && !Arena.Intersects(player.Hitbox))
			{
				allInArena = false;
			}
		}

		if (!BossSpawned && allInArena)
		{
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X, Arena.Center.Y - 400, NPCID.BrainofCthulhu);
			BossSpawned = true;

			Main.spawnTileX = Arena.Center.X / 16;
			Main.spawnTileY = Arena.Center.Y / 16;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.BrainofCthulhu) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(30, 100);
			int proj = Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.BrainofCthulhu);
			ReadyToExit = true;
		}
	}

	public class BrainSceneEffect : ModSceneEffect
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
		public override int Music => MusicID.Hell;

		public override bool IsSceneEffectActive(Player player)
		{
			return SubworldSystem.Current is BrainDomain;
		}
	}
}
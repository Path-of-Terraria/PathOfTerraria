using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.Systems.DisableBuilding;
using Terraria.DataStructures;
using Terraria.Localization;
using Steamworks;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class SkeletronDomain : BossDomainSubworld
{
	public override int Width => 600;
	public override int Height => 1000;

	public Rectangle Arena = Rectangle.Empty;
	public Point WellBottom = Point.Zero;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenTerrain),
		new PassLegacy("Arena", SpawnArena),
		new PassLegacy("Tunnels", DigTunnels)];

	private void DigTunnels(GenerationProgress progress, GameConfiguration configuration)
	{
		const int Depth = 90;

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");
		FastNoiseLite noise = GetGenNoise();

		for (int y = WellBottom.Y - 1; y < WellBottom.Y + Depth; ++y)
		{
			for (int x = WellBottom.X - 5; x < WellBottom.X + 5; ++x)
			{
				Tile tile = Main.tile[x, y];

				tile.ClearEverything();
				
				if (x < WellBottom.X - 3 || x >= WellBottom.X + 3)
				{
					tile.TileType = TileID.GrayBrick;
					tile.HasTile = true;
				}

				tile.WallType = WallID.GrayBrick;
			}

			WellBottom.X += (int)(noise.GetNoise(0, y) * 2);
		}

		CreateRoom(WellBottom.X, WellBottom.Y + Depth, WorldGen.genRand.Next(17, 23), WorldGen.genRand.Next(12, 16), true);
	}

	private static void CreateRoom(int x, int y, int width, int height, bool dontPlace)
	{
		ShapeData shapeData = new();
		x -= width / 2;
		y -= height / 2;

		WorldUtils.Gen(new Point(x, y), new Shapes.Rectangle(width, height), Actions.Chain( // Clear & add walls
			new Actions.Clear().Output(shapeData),
			new Actions.PlaceWall(WallID.BlueDungeon)
		));

		for (int i = x - 6; i <= x + width + 6; ++i)
		{
			for (int j = y - 6; j <= y + height + 6; ++j)
			{
				if (i < x || j < y || i >= x + width || j >= y + height)
				{
					Tile tile = Main.tile[i, j];

					if (dontPlace && !tile.HasTile)
					{
						continue;
					}

					tile.HasTile = true;
					tile.TileType = TileID.BlueDungeonBrick;
				}
			}
		}
	}

	private void SpawnArena(GenerationProgress progress, GameConfiguration configuration)
	{
		int y = 100;

		while (!Main.tile[Width / 2, y].HasTile)
		{
			y++;
		}

		StructureTools.PlaceByOrigin("Assets/Structures/SkeletronDomain/SkeletronWell", new Point16(Width / 2, y + 3), new Vector2(0.5f, 1));
		WellBottom = new Point(Width / 2, y + 3);

		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Assets/Structures/SkeletronArena", Mod, ref size);
		var position = new Point16(Width / 2 - size.X / 2, Height - 150 - size.Y / 2);
		StructureHelper.Generator.GenerateStructure("Assets/Structures/SkeletronArena", position, Mod);

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
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			float noiseOffset = noise.GetNoise(x, 0) * 3;
			float useY = baseY + noiseOffset;

			for (int y = (int)useY; y < Main.maxTilesY; ++y)
			{
				WorldGen.PlaceTile(x, y, y > 400 + noiseOffset ? TileID.Stone : TileID.Dirt);

				if (y > useY + 4)
				{
					WorldGen.PlaceWall(x, y, WallID.Stone, true);
				}
			}

			progress.Value = (float)x / Main.maxTilesX;
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");
		progress.Value = 0;
	}

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.02f);
		return noise;
	}

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;

		Main.dayTime = false;
		Main.time = Main.nightLength / 2;
	}

	public override void Update()
	{
		Main.dayTime = false;
		Main.time = Main.nightLength / 2;

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
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X, Arena.Center.Y, NPCID.SkeletronHead, 1);
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.SkeletronHead) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(0, 240);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.EaterofWorldsHead);
			ReadyToExit = true;
		}
	}
}

using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.Systems.DisableBuilding;
using SubworldLibrary;
using Terraria.Enums;
using Terraria.Localization;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class EyeDomain : BossDomainSubworld
{
	public const int ArenaX = 620;

	public override int Width => 800;
	public override int Height => 400;

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenSurface),
		new PassLegacy("Grass", PlaceGrassAndDecor)];

	private void PlaceGrassAndDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point16, OpenFlags> tiles = [];
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			for (int j = 80; j < Main.maxTilesY - 60; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile || tile.TileType != TileID.Dirt || tiles.ContainsKey(new Point16(i, j)))
				{
					continue;
				}

				OpenFlags flags = OpenExtensions.GetOpenings(i, j);

				if (flags == OpenFlags.None)
				{
					continue;
				}

				tiles.Add(new Point16(i, j), flags);
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		int arenaY = 0;
		HashSet<Point16> grasses = [];

		foreach ((Point16 position, OpenFlags tile) in tiles)
		{
			TrySpreadGrassOnTile(tile, position, grasses);

			if (position.X == ArenaX && Main.tile[position].HasTile && Main.tile[position].TileType == TileID.Grass)
			{
				arenaY = position.Y;
			}
		}

		foreach (Point16 position in grasses)
		{
			if (Main.tile[position].TileType == TileID.FleshBlock && WorldGen.genRand.NextBool(20))
			{ 
				WorldGen.PlaceObject(position.X, position.Y - 1, ModContent.TileType<EmbeddedEye>(), true, WorldGen.genRand.Next(2));
				continue;
			}

			if (!WorldGen.genRand.NextBool(3))
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Plants);
			}
			else if (WorldGen.genRand.NextBool(6) && position.X is > 20 and < 760)
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Saplings);
				
				if (!WorldGen.GrowTree(position.X, position.Y - 1))
				{
					WorldGen.KillTile(position.X, position.Y - 1);
				}
			}
			else if (WorldGen.genRand.NextBool(4))
			{
				WorldGen.PlaceSmallPile(position.X, position.Y - 1, WorldGen.genRand.Next(10), 0);
			}
		}

		var dims = new Point16();
		StructureHelper.Generator.GetDimensions("Assets/Structures/EyeArena", Mod, ref dims);
		StructureHelper.Generator.GenerateStructure("Assets/Structures/EyeArena", new Point16(ArenaX, arenaY - 27), Mod);
		Arena = new Rectangle(ArenaX * 16, (arenaY + 2) * 16, dims.X * 16, (dims.Y - 2) * 16);
	}

	private static void TrySpreadGrassOnTile(OpenFlags adjacencies, Point16 position, HashSet<Point16> grasses)
	{
		Tile tile = Main.tile[position];

		if (adjacencies == OpenFlags.Above)
		{
			tile.TileType = TileID.Grass;

			if (!StepX(position.X) && position.X > 150 && WorldGen.genRand.NextBool(15))
			{
				WorldGen.TileRunner(position.X, position.Y, WorldGen.genRand.NextFloat(12, 26), WorldGen.genRand.Next(12, 40), TileID.FleshBlock);
			}
			else
			{
				grasses.Add(position);
			}
		}
	}

	private void GenSurface(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = 80;
		Main.spawnTileY = 210;
		Main.worldSurface = 230;
		Main.rockLayer = 299;

		float baseY = 220;
		FastNoiseLite noise = GetGenNoise();
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			if (StepX(x))
			{
				baseY -= WorldGen.genRand.NextFloat(0.8f);
			}

			float useY = baseY + noise.GetNoise(x, 0) * 4;

			for (int y = (int)useY; y < Main.maxTilesY; ++y)
			{
				WorldGen.PlaceTile(x, y, TileID.Dirt);
			}

			progress.Value = (float)x / Main.maxTilesX;
		}
	}

	private static bool StepX(int x)
	{
		return x is > 100 and < 150 || x is > 300 and < 400 || x is > 500 and < 550;
	}

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.01f);
		return noise;
	}

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;
	}

	public override bool GetLight(Tile tile, int x, int y, ref FastRandom rand, ref Vector3 color)
	{
		if (!Main.tile[x, y].HasTile)
		{
			color = Vector3.Max(color, new Vector3(0.2f, 0.2f, 0.2f));
		}

		return true;
	}

	public override void Update()
	{
		TileEntity.UpdateStart();

		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();
		Main.dayTime = false;
		Main.time = Main.nightLength / 2;
		Main.moonPhase = (int)MoonPhase.Full;

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

			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X - 130, Arena.Center.Y - 400, NPCID.EyeofCthulhu);
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.EyeofCthulhu) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(-130, -300);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.EyeofCthulhu);
			ReadyToExit = true;
		}
	}

	public class EyeSceneEffect : ModSceneEffect
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
		public override int Music => MusicID.Eerie;

		public override bool IsSceneEffectActive(Player player)
		{
			return SubworldSystem.Current is EyeDomain;
		}
	}
}
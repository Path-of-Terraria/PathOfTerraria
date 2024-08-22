using PathOfTerraria.Content.Projectiles;
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
using PathOfTerraria.Common.World;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class BrainDomain : BossDomainSubworld
{
	public const int ArenaX = 620;

	public override int Width => 800;
	public override int Height => 1500;

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenSurface),
		new PassLegacy("Arena", SpawnArena)];

	private void SpawnArena(GenerationProgress progress, GameConfiguration configuration)
	{
		var dims = new Point16();
		StructureHelper.Generator.GetDimensions("Assets/Structures/BrainArena", Mod, ref dims);
		StructureHelper.Generator.GenerateStructure("Assets/Structures/BrainArena", new Point16(400 - dims.X / 2, 200 - dims.Y / 2), Mod);
	}

	private void GenSurface(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = 80;
		Main.spawnTileY = Height - 180;
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
				WorldGen.PlaceTile(x, y, TileID.Dirt);
			}

			progress.Value = (float)x / Main.maxTilesX;
		}

		GenerateSteps(progress, configuration);
	}

	private void GenerateSteps(GenerationProgress progress, GameConfiguration configuration)
	{
		int lineCount = 5;
		int x = 400 + WorldGen.genRand.Next(1, 5) * (WorldGen.genRand.NextBool(2) ? -1 : 1);
		int y = Height - 210;
		Queue<(Point16, Point16)> lines = [];

		for (int i = 0; i < lineCount; ++i)
		{
			if (i != 0)
			{
				if (x > 400)
				{
					x -= WorldGen.genRand.Next(120, 160);
				}
				else
				{
					x += WorldGen.genRand.Next(120, 160);
				}

				y -= WorldGen.genRand.Next(16, 50);
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

		(Point16 lastStart, Point16 lastEnd) = lines.Peek();

		foreach ((Point16 start, Point16 end) in lines) 
		{
			Vector2[] basePoints = [start.ToVector2(), Vector2.Lerp(start.ToVector2(), end.ToVector2(), 0.5f), end.ToVector2()];
			Vector2[] points = Tunnel.CreateEquidistantSet(basePoints, 46);
			float startSize = WorldGen.genRand.NextFloat(80, 110);
			float endSize = WorldGen.genRand.NextFloat(80, 110);
			float startAngle = basePoints[0].AngleTo(basePoints[2]) - MathHelper.PiOver2;
			float endAngle = startAngle + WorldGen.genRand.NextFloat(MathHelper.PiOver2);

			for (int i = 0; i < points.Length; i++)
			{
				float factor = i / (points.Length - 1f);
				GenOval(points[i], MathHelper.Lerp(startSize, endSize, factor), MathHelper.Lerp(startAngle, endAngle, factor));
			}


			progress.Value += 1 / (float)lines.Count;
		}

		Vector2 pos = Vector2.Lerp(lastStart.ToVector2(), lastEnd.ToVector2(), 1.1f);
		//Structure
	}

	private static void GenOval(Vector2 origin, float size, float angle)
	{
		var otherEnd = (origin + new Vector2(size, size / 2)).ToPoint16();
		List<Point16> results = [];
		Ellipse.Fill((x, y) => WorldGen.PlaceTile(x, y, TileID.Crimstone), origin.ToPoint16(), size, size / WorldGen.genRand.NextFloat(2, 3), angle - MathHelper.PiOver2, ref results);
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
			color = Vector3.Max(color, Color.White.ToVector3());
		}

		return true;
	}

	public override void Update()
	{
		//TileEntity.UpdateStart();

		//foreach (TileEntity te in TileEntity.ByID.Values)
		//{
		//	te.Update();
		//}

		//TileEntity.UpdateEnd();
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
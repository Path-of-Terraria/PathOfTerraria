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

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class EaterDomain : BossDomainSubworld
{
	public override int Width => 800;
	public override int Height => 1000;

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;
	public List<Vector2> SlimePositions = [];

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenSurface),
		new PassLegacy("Arena", SpawnArena)];

	private void SpawnArena(GenerationProgress progress, GameConfiguration configuration)
	{
		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Assets/Structures/EaterArena", Mod, ref size);
		StructureHelper.Generator.GenerateStructure("Assets/Structures/EaterArena", new Point16(400 - size.X / 2, Height - 150 - size.Y / 2), Mod);
	}

	private void GenSurface(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = WorldGen.genRand.NextBool() ? 80 : Main.maxTilesX - 80;
		Main.spawnTileY = 210;
		Main.worldSurface = 230;
		Main.rockLayer = 299;

		float baseY = 200;

		FastNoiseLite noise = GetGenNoise();

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			float noiseOffset = noise.GetNoise(x, 0) * 3;
			float useY = baseY + noiseOffset;

			for (int y = (int)useY; y < Main.maxTilesY; ++y)
			{
				WorldGen.PlaceTile(x, y, y > 400 + noiseOffset ? TileID.Ebonstone : TileID.Dirt);
			}
		}

		Vector2[] positions = Tunnel.GeneratePoints(GenerateWindingTunnel(400, baseY, 400, baseY + 200), 20, 10);

		foreach (Vector2 item in positions)
		{
			float mul = 1f + MathF.Abs(noise.GetNoise(item.X, item.Y)) * 1.2f;
			Digging.CircleOpening(item, 5 * mul);
			Digging.CircleOpening(item, WorldGen.genRand.Next(3, 7) * mul);

			if (WorldGen.genRand.NextBool(8))
			{
				Digging.WallCircleOpening(item, WorldGen.genRand.Next(4, 7));
			}

			if (WorldGen.genRand.NextBool(3, 5))
			{
				WorldGen.digTunnel(item.X, item.Y, 0, 0, 5, (int)(WorldGen.genRand.NextFloat(1, 8) * mul));
			}
		}
	}

	private Vector2[] GenerateWindingTunnel(int x, float y, int targetX, float targetY)
	{
		const int MaxSteps = 4;

		List<Vector2> positions = [new Vector2(x, y)];

		for (int i = 0; i < MaxSteps - 1; ++i)
		{
			var lerp = Vector2.Lerp(new Vector2(x, y), new Vector2(targetX, targetY), i / (MaxSteps - 1f));
			lerp += new Vector2(WorldGen.genRand.Next(-100, 100), WorldGen.genRand.Next(-20, 20));
			positions.Add(lerp);
		}

		positions.Add(new Vector2(targetX, targetY));
		return [.. positions];
	}

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite();
		noise.SetFrequency(0.008f);
		return noise;
	}

	private void ResetStep(GenerationProgress progress, GameConfiguration configuration)
	{
		WorldGen._lastSeed = DateTime.Now.Second;
		WorldGen._genRand = new UnifiedRandom(DateTime.Now.Second);
		WorldGen._genRand.SetSeed(DateTime.Now.Second);
	}

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;
		SlimePositions.Clear();
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

			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X - 130, Arena.Center.Y - 400, NPCID.EyeofCthulhu);
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.EaterofWorldsBody) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(-130, -300);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.EaterofWorldsHead);
			ReadyToExit = true;
		}
	}
}
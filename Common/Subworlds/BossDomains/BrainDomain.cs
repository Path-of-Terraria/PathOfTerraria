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

public class BrainDomain : BossDomainSubworld
{
	public const int ArenaX = 620;

	public override int Width => 800;
	public override int Height => 1000;

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
		Main.spawnTileY = 810;
		Main.worldSurface = 810;
		Main.rockLayer = 830;
		float baseY = 800;
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
			color = Vector3.Max(color, new Vector3(0.4f, 0.1f, 0.1f));
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
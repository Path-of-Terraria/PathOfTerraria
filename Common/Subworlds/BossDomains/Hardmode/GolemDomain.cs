using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Projectiles.Utility;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class GolemDomain : BossDomainSubworld
{
	public override int Width => 600;
	public override int Height => 1200;
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, true);

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain),
		new PassLegacy("Temples", Temples),
		new PassLegacy("Decor", Decor)];

	private void Temples(GenerationProgress progress, GameConfiguration configuration)
	{
	}

	private void Decor(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point16, OpenFlags> grasses = [];

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
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		foreach (KeyValuePair<Point16, OpenFlags> grass in grasses)
		{
			Decoration.GrowOnJungleGrass(grass.Key.X, grass.Key.Y, grass.Value);
		}
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		BossSpawned = false;
		ExitSpawned = false;

		progress.Start(1);
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Main.worldSurface = 290;
		Main.rockLayer = 300;

		FastNoiseLite noise = new FastNoiseLite();
		noise.SetFrequency(0.01f);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (j > 110 + Math.Abs(i - Width / 2) * 0.43f + noise.GetNoise(i, 0) * 7)
				{
					tile.HasTile = true;
					tile.TileType = TileID.Mud;
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}
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

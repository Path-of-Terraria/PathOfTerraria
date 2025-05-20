using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class PlanteraDomain : BossDomainSubworld
{
	public override int Width => 1700;
	public override int Height => 800;
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, false);
	public override int[] WhitelistedExplodableTiles => [ModContent.TileType<ExplosivePowder>()];
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<GrabberAnchor>()];

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;
	private static bool LeftSpawn = false;
	private static Rectangle Arena = Rectangle.Empty;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain),
		];

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Start(1);
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Main.worldSurface = 20;
		Main.rockLayer = 30;

		Vector2 center = new Vector2(Width / 2, Height / 2);
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.03f);

		FastNoiseLite cell = new (WorldGen._genRandSeed);
		cell.SetNoiseType(FastNoiseLite.NoiseType.Value);
		cell.SetFrequency(0.06f);
		cell.SetDomainWarpType(FastNoiseLite.DomainWarpType.BasicGrid);

		LeftSpawn = WorldGen.genRand.NextBool();

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				float value = cell.GetNoise(i, j) - MathHelper.Lerp(noise.GetNoise(i, j), 1, 0.2f);
				Tile tile = Main.tile[i, j];
				float dist = MathTools.ModDistance(new Vector2(i, j), center, 1, 8);

				value = MathHelper.Lerp(value, 1, GetAdjustmentBasedOnDistance(dist * (1 + MathHelper.Lerp(noise.GetNoise(i, j), 0, 0.4f))));
				tile.ClearEverything();

				float wallValue = noise.GetNoise(i, j + 8000);
				float emptyWallValue = noise.GetNoise(i, j + 16000);
				tile.WallType = wallValue > 0.4f ? WallID.JungleUnsafe : emptyWallValue < 0.2f ? WallID.None : WallID.MudUnsafe;

				if (value > 0)
				{
					tile.TileType = TileID.Mud;
					tile.HasTile = true;
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

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
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		foreach (KeyValuePair<Point16, OpenFlags> grass in grasses)
		{
			PlaceGrass(grass.Key.X, grass.Key.Y, grass.Value);
		}
	}

	private static void PlaceGrass(short x, short y, OpenFlags flags)
	{
		if (flags.HasFlag(OpenFlags.Above))
		{
			new CheckChain((int x, int y, out int? checkType) =>
			{
				checkType = 233;

				if (WorldGen.genRand.NextBool(2))
				{
					return;
				}

				WorldGen.PlaceJunglePlant(x, y, 233, WorldGen.genRand.Next(8), 0);
			}).Chain((int x, int y, out int? checkType) =>
			{
				checkType = 233;

				if (WorldGen.genRand.NextBool(2))
				{
					return;
				}

				WorldGen.PlaceJunglePlant(x, y, 233, WorldGen.genRand.Next(12), 1);
			}).Chain((int x, int y, out int? checkType) =>
			{
				checkType = WorldGen.genRand.NextBool(3) ? TileID.JunglePlants2 : TileID.JunglePlants;
				WorldGen.PlaceTile(x, y, checkType.Value, true, false, style: WorldGen.genRand.Next(24));
			}).Run(x, y - 1);
		}

		if (flags.HasFlag(OpenFlags.Below))
		{
			if (!WorldGen.genRand.NextBool(4))
			{
				int length = WorldGen.genRand.Next(5, 12);

				for (int k = 1; k < length; ++k)
				{
					if (Main.tile[x, y + k].HasTile)
					{
						break;
					}

					WorldGen.PlaceTile(x, y + k, TileID.JungleVines, true);
				}
			}
		}
	}

	private float GetAdjustmentBasedOnDistance(float dist)
	{
		return MathHelper.Clamp((dist - 600) / 200f, 0, 1);
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

		if (!BossSpawned)
		{
			bool canSpawn = Main.CurrentFrameFlags.ActivePlayersCount > 0;
			HashSet<int> who = [];

			if (canSpawn)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (!Arena.Intersects(player.Hitbox))
					{
						canSpawn = false;
						break;
					}
					else
					{
						who.Add(player.whoAmI);
					}
				}
			}

			if (canSpawn && Main.CurrentFrameFlags.ActivePlayersCount > 0 && who.Count > 0)
			{
				int plr = Main.rand.Next([.. who]);
				IEntitySource src = Entity.GetSource_NaturalSpawn();
				
				int npc = NPC.NewNPC(src, (int)Arena.Center().X, (int)Arena.Center().Y - 25, NPCID.SkeletronPrime);
				Main.npc[npc].GetGlobalNPC<ArenaEnemyNPC>().Arena = true;

				Main.spawnTileX = (int)Arena.Center().X / 16;
				Main.spawnTileY = (int)Arena.Center().Y / 16;

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.WorldData);
					NetMessage.SendTileSquare(-1, Arena.X / 16 + 72, Arena.Y / 16, 20, 1);
				}

				BossSpawned = true;
			}
		}
		else
		{
			if (!NPC.AnyNPCs(NPCID.SkeletronPrime) && !ExitSpawned)
			{
				ExitSpawned = true;

				IEntitySource src = Entity.GetSource_NaturalSpawn();
				Projectile.NewProjectile(src, Arena.Center() - new Vector2(0, 60), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
			}
		}
	}
}

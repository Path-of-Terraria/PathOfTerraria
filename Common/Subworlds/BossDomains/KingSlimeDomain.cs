using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using Terraria.Localization;
using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class KingSlimeDomain : BossDomainSubworld
{
	public override int Width => 500;
	public override int Height => 600;
	public override int[] WhitelistedCutTiles => [ModContent.TileType<EmbeddedSlimes>(), ModContent.TileType<FallingSlime>()];

	internal static Point16 ArenaEntrance = Point16.Zero;

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;
	public List<Vector2> SlimePositions = [];

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new FlatWorldPass(100, true, GetGenNoise()),
		new PassLegacy("Tunnel", TunnelGen),
		new PassLegacy("Decor", DecorGen)];

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.03f);
		return noise;
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ReadyToExit = false;
		SlimePositions.Clear();
	}

	private void DecorGen(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point16, OpenFlags> tiles = [];
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		for (int i = 60; i < Main.maxTilesX - 60; ++i)
		{
			for (int j = 100; j < Main.maxTilesY - 100; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (WorldGen.genRand.NextBool())
				{
					Tile.SmoothSlope(i, j, false);
				}

				if (!tile.HasTile || tile.TileType != TileID.Stone || tiles.ContainsKey(new Point16(i, j)))
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

		foreach ((Point16 position, OpenFlags tile) in tiles)
		{
			PlaceDecorOnTile(tile, position, true);
		}

		foreach ((Point16 position, OpenFlags tile) in tiles)
		{
			PlaceDecorOnTile(tile, position, false);
		}

		tiles.Clear();
	}

	private void PlaceDecorOnTile(OpenFlags flags, Point16 position, bool replaceTiles)
	{
		if (Main.tile[position].TileType == TileID.Stone)
		{
			if (!replaceTiles)
			{
				bool nearSlimePosition = SlimePositions.Any(x => x.DistanceSQ(position.ToVector2()) < 50 * 50);

				if (flags.HasFlag(OpenFlags.Below))
				{
					if (nearSlimePosition && WorldGen.genRand.NextBool(10))
					{
						WorldGen.PlaceTile(position.X, position.Y + 1, ModContent.TileType<FallingSlime>());
					}
					else if (WorldGen.genRand.NextBool(14))
					{
						WorldGen.PlaceTile(position.X, position.Y + 1, TileID.Stalactite);
					}
					else if (WorldGen.genRand.NextBool(60))
					{
						WorldGen.PlaceObject(position.X, position.Y + 1, TileID.Banners, true, WorldGen.genRand.NextBool(2) ? 0 : 2);
					}
				}

				if (flags.HasFlag(OpenFlags.Above))
				{
					if (nearSlimePosition && WorldGen.genRand.NextBool(5))
					{
						WorldGen.PlaceObject(position.X, position.Y - 1, ModContent.TileType<EmbeddedSlimes>(), true, WorldGen.genRand.Next(3));
					}
					else if (WorldGen.genRand.NextBool(14))
					{
						int pile = WorldGen.genRand.Next(3) switch
						{
							0 => 0,
							1 => 28,
							_ => 11
						};
						WorldGen.PlaceSmallPile(position.X, position.Y - 1, WorldGen.genRand.Next(6), 0);
					}
					else if (WorldGen.genRand.NextBool(60))
					{
						WorldGen.PlaceObject(position.X, position.Y - 2, TileID.Tombstones, true, WorldGen.genRand.Next(6));
					}
				}
			}
			else
			{
				if (flags.HasFlag(OpenFlags.Below) && WorldGen.genRand.NextBool(20))
				{
					int y = position.Y + 1;

					while (!WorldGen.SolidTile(position.X, y))
					{
						Tile tile = Main.tile[position.X, y++];
						tile.WallType = WallID.GoldBrick;
					}
				}

				int chance = 90;
				float dist = Vector2.Distance(position.ToVector2(), ArenaEntrance.ToVector2());

				if (dist < 150)
				{
					chance = (int)Math.Max(dist / 2 - 30, 1);
				}

				if (WorldGen.genRand.NextBool(chance))
				{
					WorldGen.TileRunner(position.X, position.Y, WorldGen.genRand.Next(6, 20), 8, TileID.SlimeBlock, false, 0, 0, false);
				}
			}
		}
	}

	private void TunnelGen(GenerationProgress progress, GameConfiguration configuration)
	{
		const int ArenaY = 490;

		Main.spawnTileX = WorldGen.genRand.NextBool() ? 150 : 350;
		Main.spawnTileY = 95;
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");
		progress.Value = 0;

		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Assets/Structures/KingSlimeArena", Mod, ref size);

		Arena = new Rectangle((250 - size.X / 2) * 16, (ArenaY - size.Y / 2 + 4) * 16, size.X * 16, (size.Y - 4) * 16);
		ArenaEntrance = new Point16(248, ArenaY - size.Y / 2);

		bool flip = WorldGen.genRand.NextBool(2);

		// Generate base points
		Vector2[] points = [new Vector2(250, Main.spawnTileY),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(160, 190)),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(220, 250)),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(280, 310)),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(330, 350)),
			ArenaEntrance.ToVector2()];

		for (int i = 0; i < points.Length; i++)
		{
			if (i == points.Length - 1)
			{
				continue;
			}

			Vector2 item = points[i];
			SlimePositions.Add(item);
		}

		progress.Value = 0.25f;
		Vector2[] results = Tunnel.GeneratePoints(points, 60, 4);

		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.01f);

		// Actually dig tunnel
		for (int i = 0; i < results.Length; i++)
		{
			Vector2 item = results[i];
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

			progress.Value = i / (results.Length - 1f);
		}

		// Place arena
		StructureHelper.Generator.GenerateStructure("Assets/Structures/KingSlimeArena", new Point16(250 - size.X / 2, ArenaY - size.Y / 2), Mod);

		static int GenerateEdgeX(ref bool flip)
		{
			flip = !flip;
			return 250 + WorldGen.genRand.Next(70, 160) * (flip ? -1 : 1);
		}
	}

	public override void Update()
	{
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
			for (int i = -6; i < 11; ++i)
			{
				WorldGen.PlaceTile(ArenaEntrance.X + i, ArenaEntrance.Y, TileID.SlimeBlock, true, true);
			}

			int npc = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X, Arena.Center.Y + 400, NPCID.KingSlime);

			Main.spawnTileX = Arena.Center.X / 16;
			Main.spawnTileY = Arena.Center.Y / 16;

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendTileSquare(-1, ArenaEntrance.X - 6, ArenaEntrance.Y, 16, 1);
				NetMessage.SendData(MessageID.WorldData);
			}

			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.KingSlime) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(0, 150);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.KingSlime);
			ReadyToExit = true;
		}
	}
}
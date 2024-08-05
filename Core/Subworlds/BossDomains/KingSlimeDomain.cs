using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Systems.DisableBuilding;
using PathOfTerraria.Core.WorldGeneration;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

public class KingSlimeDomain : BossDomainSubworld
{
	[Flags]
	public enum Open
	{
		None = 0,
		Above,
		Below
	}

	public override int Width => 500;
	public override int Height => 600;

	internal static Point16 ArenaEntrance = Point16.Zero;

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;
	public List<Vector2> SlimePositions = [];

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new FlatWorldPass(100, true, GetGenNoise()), 
		new PassLegacy("Tunnel", TunnelGen), new PassLegacy("Decor", DecorGen)];

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite();
		noise.SetFrequency(0.03f);
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

	private void DecorGen(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point16, Open> tiles = [];

		for (int i = 60; i < Main.maxTilesX - 60; ++i)
		{
			for (int j = 100; j < Main.maxTilesY - 100; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile || tile.TileType != TileID.Stone || tiles.ContainsKey(new Point16(i, j)))
				{
					continue;
				}

				Open flags = Open.None;

				if (!Main.tile[i, j - 1].HasTile)
				{
					flags |= Open.Above;
				}

				if (!Main.tile[i, j + 1].HasTile)
				{
					flags |= Open.Below;
				}

				if (flags == Open.None)
				{
					continue;
				}

				tiles.Add(new Point16(i, j), flags);
			}
		}

		foreach ((Point16 position, Open tile) in tiles)
		{
			PlaceDecorOnTile(tile, position, false);
		}

		foreach ((Point16 position, Open tile) in tiles)
		{
			PlaceDecorOnTile(tile, position, true);
		}

		tiles.Clear();
	}

	private void PlaceDecorOnTile(Open flags, Point16 position, bool late)
	{
		if (Main.tile[position].TileType == TileID.Stone)
		{
			if (!late)
			{
				bool nearSlimePosition = SlimePositions.Any(x => x.DistanceSQ(position.ToVector2()) < 50 * 50);

				if (flags.HasFlag(Open.Below))
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

				if (flags.HasFlag(Open.Above))
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
				if (flags.HasFlag(Open.Below) && WorldGen.genRand.NextBool(20))
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

		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Data/Structures/KingSlimeArena", Mod, ref size);

		Arena = new Rectangle((250 - size.X / 2) * 16, (ArenaY - size.Y / 2 + 4) * 16, size.X * 16, (size.Y - 4) * 16);
		ArenaEntrance = new Point16(248, ArenaY - size.Y / 2);

		bool flip = WorldGen.genRand.NextBool(2);

		// Generate base points
		Vector2[] points = [new Vector2(250, Main.spawnTileY),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(160, 190)),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(220, 250)),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(280, 310)),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(330, 360)),
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

		Vector2[] results = Tunnel.GeneratePoints(points, 60, 10);

		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.01f);

		// Actually dig tunnel
		foreach (Vector2 item in results)
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

		// Place arena
		StructureHelper.Generator.GenerateStructure("Data/Structures/KingSlimeArena", new Point16(250 - size.X / 2, ArenaY - size.Y / 2), Mod);

		static int GenerateEdgeX(ref bool flip)
		{
			flip = !flip;
			return 250 + WorldGen.genRand.Next(80, 160) * (flip ? -1 : 1);
		}
	}

	public override void Update()
	{
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
			for (int i = -6; i < 11; ++i)
			{
				WorldGen.PlaceTile(ArenaEntrance.X + i, ArenaEntrance.Y, TileID.SlimeBlock, true, true);
			}

			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X, Arena.Center.Y + 400, NPCID.KingSlime);
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
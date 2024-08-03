using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Core.Subworlds.Passes;
using PathOfTerraria.Core.Systems.DisableBuilding;
using PathOfTerraria.Core.WorldGeneration;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Core.Subworlds;

public class KingSlimeDomain : MappingWorld
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

	public static Point16 ArenaEntrance = Point16.Zero;

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new FlatWorldPass(100, true), 
		new PassLegacy("Tunnel", TunnelGen), new PassLegacy("Decor", DecorGen)];

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
			PlaceDecorOnTile(tile, position);
		}

		tiles.Clear();

		WorldGen.PlaceTile(ArenaEntrance.X, ArenaEntrance.Y, TileID.Meteorite, true, true);
	}

	private static void PlaceDecorOnTile(Open flags, Point16 position)
	{
		if (Main.tile[position].TileType == TileID.Stone)
		{
			if (flags.HasFlag(Open.Below))
			{
				if (WorldGen.genRand.NextBool(14))
				{
					WorldGen.PlaceTile(position.X, position.Y + 1, TileID.Stalactite);
				}
				else if (WorldGen.genRand.NextBool(60))
				{
					WorldGen.PlaceObject(position.X, position.Y + 1, TileID.Banners, false, WorldGen.genRand.NextBool(2) ? 0 : 2);
				}
			}

			if (flags.HasFlag(Open.Above))
			{
				if (WorldGen.genRand.NextBool(14))
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

			int chance = 90;
			float dist = Vector2.Distance(position.ToVector2(), ArenaEntrance.ToVector2());
			
			if (dist < 90)
			{
				chance = (int)Math.Max(dist / 2 - 10, 1);
			}

			if (WorldGen.genRand.NextBool(chance))
			{
				WorldGen.TileRunner(position.X, position.Y, WorldGen.genRand.Next(6, 20), 8, TileID.SlimeBlock, false, 0, 0, false);
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
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(340, 370)),
			ArenaEntrance.ToVector2()];

		// Generate "slime arenas" TBD
		for (int i = 0; i < points.Length; i++)
		{
			if (i == points.Length - 1)
			{
				continue;
			}

			Vector2 item = points[i];

			WorldGen.digTunnel(item.X, item.Y, 0, 0, 5, 18);
		}

		points = AddVariationToPoints(points);
		Vector2[] results = Spline.InterpolateXY(points, 60);
		results = CreateEquidistantSet(results, 10);

		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.01f);

		// Actually dig tunnel
		foreach (Vector2 item in results)
		{
			float mul = 1f + MathF.Abs(noise.GetNoise(item.X, item.Y)) * 1.2f;
			TunnelSpot(item, 5 * mul);
			TunnelSpot(item, WorldGen.genRand.Next(3, 7) * mul);

			if (WorldGen.genRand.NextBool(8))
			{
				TunnelWallSpot(item, WorldGen.genRand.Next(4, 7));
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

	private Vector2[] AddVariationToPoints(Vector2[] points)
	{
		List<Vector2> newPoints = [];

		for (int i = 0; i < points.Length; i++)
		{
			Vector2 item = points[i];
			newPoints.Add(item);

			if (i == points.Length - 1)
			{
				continue;
			}

			if (i < points.Length - 1 && WorldGen.genRand.NextBool())
			{
				var startLerp = Vector2.Lerp(item, points[i + 1], WorldGen.genRand.NextFloat(0.3f, 0.7f));
				startLerp += item.DirectionTo(points[i + 1]).RotatedBy(MathHelper.Pi * (WorldGen.genRand.NextBool() ? -1 : 1)).RotatedByRandom(0.1f) 
					* WorldGen.genRand.NextFloat(10, 20);
				newPoints.Add(startLerp);
			}
			else
			{
				const int Variance = 40;

				newPoints.Add(item + new Vector2(WorldGen.genRand.Next(-Variance, Variance), WorldGen.genRand.Next(Variance)));
			}
		}

		return [.. newPoints];
	}

	private Vector2[] CreateEquidistantSet(Vector2[] results, float distance)
	{
		List<Vector2> points = [];
		Queue<Vector2> remainingPoints = new(results);
		Vector2 start = remainingPoints.Dequeue();
		Vector2 current = start;
		Vector2 next = remainingPoints.Dequeue();
		float factor = 0;

		while (true)
		{
			float dist = current.Distance(next);

			while (true)
			{
				points.Add(Vector2.Lerp(start, next, factor));
				factor += MathF.Min(1, distance / dist);

				if (factor > 1f)
				{
					break;
				}
			}

			if (remainingPoints.Count == 0)
			{
				return [.. points];
			}

			start = next;
			next = remainingPoints.Dequeue();
			factor--;
		}
	}

	/// <summary>
	/// Super placeholder dig method for the subworld. Really needs fancifying.
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="size"></param>
	private static void TunnelSpot(Vector2 pos, float size)
	{
		for (int i = (int)(pos.X - size); i < (int)pos.X + size; ++i)
		{
			for (int j = (int)(pos.Y - size); j < (int)pos.Y + size; ++j)
			{
				if (Vector2.DistanceSquared(pos, new Vector2(i, j)) < size * size)
				{
					WorldGen.KillTile(i, j);
				}
			}
		}
	}

	/// <summary>
	/// Super placeholder dig method for the subworld. Really needs fancifying.
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="size"></param>
	private static void TunnelWallSpot(Vector2 pos, float size)
	{
		for (int i = (int)(pos.X - size); i < (int)pos.X + size; ++i)
		{
			for (int j = (int)(pos.Y - size); j < (int)pos.Y + size; ++j)
			{
				if (Vector2.DistanceSquared(pos, new Vector2(i, j)) < size * size)
				{
					WorldGen.KillWall(i, j);
				}
			}
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

			ReadyToExit = true;
		}
	}
}
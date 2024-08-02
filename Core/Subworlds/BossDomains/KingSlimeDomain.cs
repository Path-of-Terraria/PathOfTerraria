using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Core.Subworlds.Passes;
using PathOfTerraria.Core.Systems.DisableBuilding;
using PathOfTerraria.Core.WorldGeneration;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Core.Subworlds;

public class KingSlimeDomain : MappingWorld
{
	public override int Width => 500;
	public override int Height => 600;

	public Point16 ArenaEntrance = Point16.Zero;
	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new FlatWorldPass(0, true), new PassLegacy("Tunnel", TunnelGen), new PassLegacy("Decor", DecorGen)];

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;
	}

	private void DecorGen(GenerationProgress progress, GameConfiguration configuration)
	{
		for (int i = 60; i < Main.maxTilesX - 60; ++i)
		{
			for (int j = 100; j < Main.maxTilesY - 100; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile)
				{
					continue;
				}

				if (tile.Slope == SlopeType.Solid)
				{
					PlaceDecorOnTile(i, j, tile);
				}
			}
		}
	}

	private void PlaceDecorOnTile(int i, int j, Tile tile)
	{
		if (tile.TileType == TileID.Stone)
		{
			if (Main.tile[i, j + 1].HasTile && WorldGen.genRand.NextBool(14))
			{
				WorldGen.PlaceTile(i, j + 2, TileID.Stalactite);
			}

			if (Main.tile[i, j - 1].HasTile && WorldGen.genRand.NextBool(14))
			{
				WorldGen.PlaceSmallPile(i, j - 1, 2, 0);
			}
		}
	}

	private void TunnelGen(GenerationProgress progress, GameConfiguration configuration)
	{
		WorldGen._genRand = new Terraria.Utilities.UnifiedRandom(Main.rand.Next());

		Main.spawnTileX = 250;
		Main.spawnTileY = 500;

		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Data/Structures/KingSlimeArena", Mod, ref size);

		Arena = new Rectangle((250 - size.X / 2) * 16, (120 - size.Y / 2) * 16, size.X * 16, (size.Y - 4) * 16);
		ArenaEntrance = new Point16(255, 120 + size.Y / 2);

		bool flip = false;

		Vector2[] points = [new Vector2(Main.spawnTileX, Main.spawnTileY),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(420, 450)),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(360, 390)),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(300, 330)),
			new Vector2(GenerateEdgeX(ref flip), WorldGen.genRand.Next(240, 270)),
			ArenaEntrance.ToVector2()];
		Vector2[] results = Spline.InterpolateXY(points, 60);
		results = CreateEquidistantSet(results, 10);

		foreach (Vector2 item in results)
		{
			TunnelSpot(item, 6);
		}

		StructureHelper.Generator.GenerateStructure("Data/Structures/KingSlimeArena", new Point16(250 - size.X / 2, 120 - size.Y / 2), Mod);

		static int GenerateEdgeX(ref bool flip)
		{
			flip = !flip;
			return 250 + WorldGen.genRand.Next(150, 200) * (flip ? -1 : 1);
		}
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

	private void Excavate(Vector2 pos, Queue<Vector2> queue, int repeats, int fuzzSize)
	{
		for (int i = 0; i < repeats; ++i)
		{
			DigThroughTo(pos, new(queue), 4, fuzzSize);
		}
		DigThroughTo(pos, queue, 5);
	}

	private void DigThroughTo(Vector2 pos, Queue<Vector2> stopPoints, int size, int? fuzz = null)
	{
		Vector2 end = stopPoints.Dequeue();

		if (fuzz.HasValue)
		{
			end += new Vector2(WorldGen.genRand.Next(-fuzz.Value, fuzz.Value), WorldGen.genRand.Next(-fuzz.Value, fuzz.Value));
		}

		while (true)
		{
			TunnelSpot(pos, size);

			pos += Vector2.Normalize(end - pos);

			if (Vector2.DistanceSquared(pos, end) < 4)
			{
				if (stopPoints.Count == 0)
				{
					return;
				}

				end = stopPoints.Dequeue();

				if (fuzz.HasValue)
				{
					end += new Vector2(WorldGen.genRand.Next(-fuzz.Value, fuzz.Value), WorldGen.genRand.Next(-fuzz.Value, fuzz.Value));
				}
			}
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
				WorldGen.PlaceTile(ArenaEntrance.X + i, ArenaEntrance.Y - 1, TileID.SlimeBlock, true, true);
			}

			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X, Arena.Center.Y, NPCID.KingSlime);
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.KingSlime) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(0, 350);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			ReadyToExit = true;
		}
	}
}
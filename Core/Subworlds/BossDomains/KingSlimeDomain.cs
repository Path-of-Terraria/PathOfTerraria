using PathOfTerraria.Core.Subworlds.Passes;
using PathOfTerraria.Core.Systems.DisableBuilding;
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

	public override List<GenPass> Tasks => [new FlatWorldPass(0, true), new PassLegacy("Tunnel", TunnelGen)];

	public override void OnEnter()
	{
		BossSpawned = false;
	}

	private void TunnelGen(GenerationProgress progress, GameConfiguration configuration)
	{
		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Data/Structures/KingSlimeArena", Mod, ref size);

		Arena = new Rectangle((250 - size.X / 2) * 16, (120 - size.Y / 2) * 16, size.X * 16, (size.Y - 4) * 16);
		ArenaEntrance = new Point16(255, 120 + size.Y / 2);

		Main.spawnTileX = 250;
		Main.spawnTileY = 500;

		int noiseX = 0;
		var pos = new Vector2(Main.spawnTileX, Main.spawnTileY);
		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.08f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

		Queue<Vector2> queue = [];
		queue.Enqueue(new Vector2(Main.rand.Next(100, 400), Main.rand.Next(420, 450)));
		queue.Enqueue(new Vector2(Main.rand.Next(100, 400), Main.rand.Next(350, 380)));
		queue.Enqueue(new Vector2(Main.rand.Next(100, 400), Main.rand.Next(280, 320)));
		queue.Enqueue(ArenaEntrance.ToVector2());

		DigThroughTo(ref noiseX, pos, noise, queue);

		StructureHelper.Generator.GenerateStructure("Data/Structures/KingSlimeArena", new Point16(250 - size.X / 2, 120 - size.Y / 2), Mod);
	}

	private void DigThroughTo(ref int noiseX, Vector2 pos, FastNoiseLite noise, Queue<Vector2> stopPoints)
	{
		Vector2 end = stopPoints.Dequeue();

		while (true)
		{
			TunnelSpot(pos, noise.GetNoise(noiseX++, 0) * 16 + 8);

			pos += Vector2.Normalize(end - pos);

			if (Vector2.DistanceSquared(pos, end) < 4)
			{
				if (stopPoints.Count == 0)
				{
					return;
				}

				end = stopPoints.Dequeue();
			}
		}
	}

	private void TunnelSpot(Vector2 pos, float size)
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
	}
}
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class CultistDomain : BossDomainSubworld
{
	const int FloorY = 400;
	const int PedestalDistance = 400;
	const int EdgeDistance = 800;

	public override int Width => 1900;
	public override int Height => 600;
	public override (int time, bool isDay) ForceTime => ((int)(Main.dayLength * 0.95), true);
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<TabletPieces>()];

	public static Point16 BulbPosition = new();
	public static int BulbsBroken = 0;

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new FlatWorldPass(FloorY, tileType: TileID.BlueDungeonBrick),
		new PassLegacy("Structures", GenTerrain)];

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		BulbsBroken = 0;
		BossSpawned = false;
		ExitSpawned = false;

		progress.Start(1);
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Point16 cliffSize = StructureTools.GetSize("Assets/Structures/LunaticDomain/CliffLeft");
		int cliffWidth = cliffSize.X;
		int cliffHeight = cliffSize.Y - 1;

		for (int i = 0; i < Width / 2 - EdgeDistance; ++i)
		{
			for (int j = FloorY - cliffHeight; j < Height - 10; ++j)
			{
				Tile tile = Main.tile[i, j];
				tile.HasTile = true;
				tile.TileType = TileID.BlueDungeonBrick;

				tile = Main.tile[Width / 2 + EdgeDistance + i, j];
				tile.HasTile = true;
				tile.TileType = TileID.BlueDungeonBrick;
			}
		}

		StructureTools.PlaceByOrigin("Assets/Structures/LunaticDomain/Center_" + WorldGen.genRand.Next(2), GetFloor(Width / 2, 200), new Vector2(0.5f, 1));

		PriorityQueue<int, float> pieces = new();
		pieces.Enqueue(0, WorldGen.genRand.NextFloat());
		pieces.Enqueue(1, WorldGen.genRand.NextFloat());
		pieces.Enqueue(2, WorldGen.genRand.NextFloat());
		pieces.Enqueue(3, WorldGen.genRand.NextFloat());

		PlaceStructureWithPiece("Pedestal_" + WorldGen.genRand.Next(2), Width / 2 - PedestalDistance, new Vector2(0.5f, 1), pieces.Dequeue());
		PlaceStructureWithPiece("Pedestal_" + WorldGen.genRand.Next(2), Width / 2 + PedestalDistance, new Vector2(0.5f, 1), pieces.Dequeue());

		PlaceStructureWithPiece("CliffLeft", Width / 2 - EdgeDistance, new Vector2(0, 1), pieces.Dequeue());
		PlaceStructureWithPiece("CliffRight", Width / 2 + EdgeDistance, new Vector2(1), pieces.Dequeue());
	}

	private static void PlaceStructureWithPiece(string structure, int x, Vector2 origin, int style)
	{
		StructureTools.PlaceByOrigin("Assets/Structures/LunaticDomain/" + structure, new Point16(x, FloorY + 1), origin);

		while (true)
		{
			int pieceX = x + WorldGen.genRand.Next(-40, 40);
			Point16 pos = GetFloor(pieceX, 200);

			WorldGen.PlaceObject(pos.X, pos.Y - 1, ModContent.TileType<TabletPieces>(), true, style);

			if (Main.tile[pos.X, pos.Y - 1].TileType == ModContent.TileType<TabletPieces>() && Main.tile[pos.X, pos.Y - 1].HasTile)
			{
				return;
			}
		}
	}

	public static Point16 GetFloor(int x, int y)
	{
		while (!WorldGen.SolidOrSlopedTile(x, y))
		{
			y++;
		}

		return new (x, y);
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

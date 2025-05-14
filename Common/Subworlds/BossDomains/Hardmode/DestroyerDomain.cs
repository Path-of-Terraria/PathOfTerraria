using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class DestroyerDomain : BossDomainSubworld
{
	[Flags]
	public enum PlatformState
	{
		None = 0,
		Above = 1,
		Below = 2,
		Both = Above | Below
	}

	public const int FloorY = 300;

	public static UnifiedRandom GenRandom => Main.rand;

	public override int Width => 1200;
	public override int Height => 400;
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, true);
	public override int[] WhitelistedExplodableTiles => [ModContent.TileType<ExplosivePowder>()];
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<GrabberAnchor>()];

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;
	private static Rectangle Arena = Rectangle.Empty;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new FlatWorldPass(FloorY, true, null, TileID.TinPlating, ModContent.WallType<TinPlatingUnsafe>()),
		new PassLegacy("City", BuildCity)];

	private void BuildCity(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileY = FloorY - 4;

		int reps = Width / 130;

		for (int i = 0; i < reps; ++i)
		{
			if (WorldGen.genRand.NextBool(6))
			{
				continue;
			}

			int x = (int)MathHelper.Lerp(120, Main.maxTilesX - 120, i / (reps - 1f)) + WorldGen.genRand.Next(-30, 30);

			MakeTower(x, FloorY + 1);
		}

		for (int i = 0; i < 90; ++i)
		{
			WorldUtils.Gen(new Point(WorldGen.genRand.Next(40, Main.maxTilesX - 40), FloorY + 2), 
				new ShapeBranch(WorldGen.genRand.NextFloat(-0.9f, 0.2f), WorldGen.genRand.NextFloat(16, 80)), new Actions.PlaceWall(WallID.EbonstoneUnsafe));

			if (i % 2 == 0)
			{
				float shapeDist = WorldGen.genRand.NextFloat(16, 80);

				WorldUtils.Gen(new Point(WorldGen.genRand.Next(40, Main.maxTilesX - 40), FloorY + 2),
					new ShapeBranch(WorldGen.genRand.NextFloat(-0.9f, 0.2f) + MathHelper.PiOver2, shapeDist), new Actions.SetTile(TileID.Ebonstone));
			}
		}
	}

	public static void MakeTower(int x, int y)
	{
		int reps = WorldGen.genRand.Next(8, 15);
		int width = WorldGen.genRand.Next(50, 60);
		bool endNext = false;
		int originalY = y;

		for (int i = 0; i < reps; ++i)
		{
			int height = Math.Max(5, (int)(width * WorldGen.genRand.NextFloat(0.3f, 0.5f)));
			bool last = i == reps - 1 || endNext;

			if (last)
			{
				width = 45;
				height = 20;
			}

			y -= height - 1;

			MakeHouse(x, y, width, height, i == 0 ? PlatformState.None : PlatformState.Below, last, i == 1);

			width -= WorldGen.genRand.Next(2, 7);

			if (last)
			{
				break;
			}

			if (width < 14)
			{
				endNext = true;
			}
		}

		GenVars.structures.AddProtectedStructure(new Rectangle(x - width / 2, originalY, width, originalY - y));
	}

	public static void MakeHouse(int x, int y, int width, int height, PlatformState platformState = PlatformState.None, bool forceCenter = false, bool openWalls = false)
	{
		x -= width / 2;

		ShapeData data = new();
		WorldUtils.Gen(new Point(x, y), new Shapes.Rectangle(width, height), new Actions.PlaceWall((ushort)ModContent.WallType<TinPlatingUnsafe>()).Output(data));
		WorldUtils.Gen(new Point(x, y), new ModShapes.InnerOutline(data), Actions.Chain(new Modifiers.Blotches(), new Actions.PlaceTile(TileID.TinPlating), new Actions.ClearWall()));

		if (platformState.HasFlag(PlatformState.Below))
		{
			int openingWidth = WorldGen.genRand.Next(Math.Max(3, width / 4), Math.Max(4, width / 2));
			int floorX = x + WorldGen.genRand.Next(width - openingWidth - 8) + 4;

			if (forceCenter)
			{
				floorX = x + width / 2 - 3;
				openingWidth = 6;
			}

			for (int j = -1; j < 2; ++j)
			{
				for (int i = floorX; i < floorX + openingWidth; ++i)
				{
					Tile tile = Main.tile[i, y + height - 1 + j];
					tile.Clear(TileDataType.Tile);
					tile.WallType = (ushort)ModContent.WallType<TinPlatingUnsafe>();
				}
			}
		}

		if (openWalls)
		{
			for (int i = -1; i < 2; ++i)
			{
				for (int j = y + 1; j < y + height - 1; ++j)
				{
					Tile tile = Main.tile[x + i, j];
					tile.Clear(TileDataType.Tile);

					tile = Main.tile[x + i + width - 1, j];
					tile.Clear(TileDataType.Tile);
				}
			}
		}

		PopulateHouse(data);
	}

	private static void PopulateHouse(ShapeData data)
	{
		foreach (Point16 pos in data.GetData())
		{

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

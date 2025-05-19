using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class DestroyerDomain : BossDomainSubworld, IOverrideBiome
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

	public override int Width => 1300;
	public override int Height => 400;
	public override (int time, bool isDay) ForceTime => (600, false);
	public override int[] WhitelistedMiningTiles => [TileID.Ebonstone, ModContent.TileType<MechCapsule>()];
	public override int[] WhitelistedCutTiles => [ModContent.TileType<TechDriveTile>()];

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;
	private static Rectangle Arena = Rectangle.Empty;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new FlatWorldPass(FloorY, true, null, TileID.TinPlating, ModContent.WallType<TinPlatingUnsafe>()),
		new PassLegacy("City", BuildCity),
		new PassLegacy("Decor", DecorateWorld)];

	private void DecorateWorld(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		HashSet<Point16> blueSpots = [];
		Dictionary<Point16, OpenFlags> metals = [];

		for (int i = 10; i < Main.maxTilesX - 10; ++i)
		{
			for (int j = 10; j < Main.maxTilesY - 10; ++j)
			{
				Tile tile = Main.tile[i, j];
				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

				if (tile.HasTile && (tile.TileType == TileID.TinPlating || tile.TileType == ModContent.TileType<MechPlatform>()))
				{
					tile.TileColor = PaintID.GrayPaint;

					if (tile.TileType == ModContent.TileType<MechPlatform>())
					{
						metals.Add(new Point16(i, j), flags);
					}
				}

				if (tile.WallType == ModContent.WallType<TinPlatingUnsafe>())
				{
					tile.WallColor = PaintID.GrayPaint;

					if (WorldGen.genRand.NextBool(460))
					{
						blueSpots.Add(new Point16(i, j));
					}
				}
			}

			progress.Set((i - 10f) / (Main.maxTilesX - 20f));
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingMetals");

		foreach (KeyValuePair<Point16, OpenFlags> metal in metals)
		{
			TwinsDomain.DecorateMetals(metal.Key, metal.Value, true);
		}

		foreach (Point16 spot in blueSpots)
		{
			for (int i = -2; i < 3; ++i)
			{
				Tile tile = Main.tile[spot.X, spot.Y + i];

				if (tile.WallType != WallID.None)
				{
					tile.WallType = (ushort)ModContent.WallType<BrokenSoftGlowplateWall>();
					tile.WallColor = PaintID.None;
				}
			}
		}
	}

	private void BuildCity(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Structures");

		int reps = Width / 130;
		int spawnTower = WorldGen.genRand.Next(reps);
		int skipped = -1;

		for (int i = 0; i < reps; ++i)
		{
			if (WorldGen.genRand.NextBool(6) && i != spawnTower && skipped < 4)
			{
				skipped++;
				continue;
			}

			int x = (int)MathHelper.Lerp(130, Main.maxTilesX - 160, i / (reps - 1f)) + WorldGen.genRand.Next(-30, 30);
			int y = MakeTower(x, FloorY + 1, spawnTower == i);

			if (spawnTower == i)
			{
				Main.spawnTileX = x + (WorldGen.genRand.NextBool() ? -1 : 1 * WorldGen.genRand.Next(5, 12));
				Main.spawnTileY = y + 16;
			}

			progress.Set(i / (float)reps);
		}

		for (int i = 0; i < 90; ++i)
		{
			var pos = new Point(WorldGen.genRand.Next(80, Main.maxTilesX - 80), FloorY + 2);
			Branch(pos);

			if (i % 2 == 0)
			{
				pos = new Point(WorldGen.genRand.Next(80, Main.maxTilesX - 80), FloorY + 2);
				Branch(pos, null, true);
			}

			progress.Set(i / 90f);
		}

		for (int i = 0; i < 12; ++i)
		{
			int x;
			int y;

			do
			{
				x = WorldGen.genRand.Next(170, Main.maxTilesX - 170);
				y = WorldGen.genRand.Next(150, FloorY - 40);
			} while (!GenVars.structures.CanPlace(new Rectangle(x, y, 30, 8)));

			FloatingHouse(x, y);
			progress.Set(i / 12f);
		}
	}

	private static void Branch(Point pos, Range? sizeRange = null, bool placeTiles = false, float forceAngle = float.PositiveInfinity)
	{
		ShapeData shape = new();

		float angle = WorldGen.genRand.NextFloat(-1.4f, 0.2f);

		if (placeTiles)
		{
			angle += MathHelper.Pi;
		}

		if (forceAngle != float.PositiveInfinity)
		{
			angle = forceAngle;
		}

		// Base branch
		WorldUtils.Gen(pos, new ShapeBranch(angle,
			sizeRange is null ? WorldGen.genRand.NextFloat(16, 80) : WorldGen.genRand.NextFloat(sizeRange.Value.Start.Value, sizeRange.Value.End.Value)), 
			placeTiles ? new Actions.SetTile(TileID.Ebonstone) : new Actions.PlaceWall(WallID.EbonstoneUnsafe).Output(shape));

		// Leaf details
		WorldUtils.Gen(pos, new ModShapes.InnerOutline(shape), Actions.Chain(new Modifiers.Blotches(6, 3, 0.05f), new Modifiers.NotInShape(shape),
			new Actions.PlaceWall(WallID.CorruptGrassUnsafe)));
	}

	public static int MakeTower(int x, int y, bool spawnTower)
	{
		int reps = WorldGen.genRand.Next(8, 18);
		int width = WorldGen.genRand.Next(50, 60);
		int originalWidth = width;
		int originalY = y;
		bool endNext = false;

		for (int i = 0; i < reps; ++i)
		{
			int height = Math.Max(7, (int)(width * WorldGen.genRand.NextFloat(0.3f, 0.5f)));
			bool last = i == reps - 1 || endNext;

			if (last)
			{
				width = 45;
				height = 20;
			}

			y -= height - 1;

			if (!last || spawnTower)
			{
				MakeHouse(x, y, width, height, i == 0 ? PlatformState.None : PlatformState.Below, last, i == 1 || WorldGen.genRand.NextBool(6) && !last, last);
			}
			else
			{
				StructureTools.PlaceByOrigin($"Assets/Structures/DestroyerDomain/Top_" + Main.rand.Next(8), new Point16(x, y), new Vector2(0.5f, 0));
			}

			width -= WorldGen.genRand.Next(1, 7);

			if (last)
			{
				break;
			}

			if (width < 14)
			{
				endNext = true;
			}
		}
		
		GenVars.structures.AddProtectedStructure(new Rectangle(x - width / 2, y, originalWidth - 6, originalY - y), 8);
		return y;
	}

	public static void MakeHouse(int x, int y, int width, int height, PlatformState platformState = PlatformState.None, bool forceCenter = false, bool openWalls = false,
		bool mechGatePlatform = false)
	{
		x -= width / 2;

		ShapeData data = new();
		WorldUtils.Gen(new Point(x, y), new Shapes.Rectangle(width, height), new Actions.PlaceWall((ushort)ModContent.WallType<TinPlatingUnsafe>()).Output(data));
		WorldUtils.Gen(new Point(x, y), new ModShapes.InnerOutline(data), Actions.Chain(new Modifiers.Blotches(), 
			new Actions.PlaceTile(TileID.TinPlating), new Actions.ClearWall()));

		if (platformState.HasFlag(PlatformState.Below))
		{
			AddPlatformHole(x, y + height - 1, width, forceCenter, mechGatePlatform);
		}

		if (platformState.HasFlag(PlatformState.Above))
		{
			AddPlatformHole(x, y, width, forceCenter);
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

		PopulateHouse(data, new Point(x, y));
	}

	private static void AddPlatformHole(int x, int y, int width, bool forceCenter, bool mechGatePlatform = false)
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
				Tile tile = Main.tile[i, y + j];
				tile.Clear(TileDataType.Tile);

				if (j == 0)
				{
					tile.TileType = (ushort)(mechGatePlatform ? ModContent.TileType<BlockingGate>() : ModContent.TileType<MechPlatform>());
					tile.HasTile = true;
				}

				tile.WallType = (ushort)ModContent.WallType<TinPlatingUnsafe>();
			}
		}
	}

	public static void FloatingHouse(int x, int y)
	{
		int width = WorldGen.genRand.Next(18, 30);
		MakeHouse(x, y, width + width % 2, 8, PlatformState.Above);

		if (WorldGen.genRand.NextBool(2) && width > 20)
		{
			PlacePulseGenerator(x - 4, y);
			PlacePulseGenerator(x + 4, y);
		}
		else
		{
			PlacePulseGenerator(x, y);
		}

		if (WorldGen.genRand.NextBool())
		{
			PlaceChest(x, y, width);
		}

		GenVars.structures.AddProtectedStructure(new Rectangle(x - width / 2, y, width, 8));
	}

	private static void PlaceChest(int x, int y, int width)
	{
		int chestX = WorldGen.genRand.Next(-width / 3, width / 3);

		for (int i = 0; i < 2; ++i)
		{
			for (int j = -2; j < 1; ++j)
			{
				int k = x + i;
				Tile tile = Main.tile[k + chestX, y + 7 + j];

				if (j == 0)
				{
					WorldGen.PlaceTile(k + chestX, y + 7 + j, TileID.TinPlating);
					tile.Slope = SlopeType.Solid;
				}
				else
				{
					tile.Clear(TileDataType.Tile);
				}
			}
		}

		WorldGen.PlaceObject(x + chestX, y + 6, ModContent.TileType<MechCapsule>(), true, 0);
	}

	private static void PlacePulseGenerator(int x, int y)
	{
		for (int i = 0; i < 6; ++i)
		{
			int k = x - 3 + i;

			WorldGen.PlaceTile(k, y + 8, TileID.TinPlating);
		}

		WorldGen.PlaceTile(x - 3, y + 9, ModContent.TileType<PulseGenerator>());
	}

	private static void PopulateHouse(ShapeData data, Point position)
	{
		HashSet<Point16> positions = data.GetData();

		foreach (Point16 pos in positions)
		{
			if (AnyAdjacentOpening(positions, pos, position) && WorldGen.genRand.NextBool(20))
			{
				float factor = WorldGen.genRand.NextFloat();
				int max = (int)MathHelper.Lerp(MathHelper.Lerp(16, 20, factor), MathHelper.Lerp(20, 34, factor), factor);
				Branch(new Point(position.X + pos.X, position.Y + pos.Y), 12..max, WorldGen.genRand.NextBool(12), WorldGen.genRand.NextFloat(-3f, 0.2f));
			}
		}
	}

	private static bool AnyAdjacentOpening(HashSet<Point16> positions, Point16 pos, Point origin)
	{
		return OpenNoWall(pos.X - 1, pos.Y) || OpenNoWall(pos.X + 1, pos.Y) || OpenNoWall(pos.X, pos.Y + 1) || OpenNoWall(pos.X, pos.Y - 1);

		bool OpenNoWall(int x, int y)
		{
			return !positions.Contains(new Point16(x, y)) && Main.tile[x + origin.X, y + origin.Y].WallType == WallID.None;
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

	public void OverrideBiome()
	{
		Main.LocalPlayer.ZoneCorrupt = true;
		Main.newMusic = MusicID.Corruption;
		Main.curMusic = MusicID.Corruption;
	}
}

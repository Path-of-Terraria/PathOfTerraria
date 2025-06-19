using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonDomainGenerationTools
{
	/// <summary>
	/// <see cref="WorldGen.GrowLivingTree(int, int, bool)"/> but with the conditions removed.<br/>
	/// This is only cleaned up enough to remove warnings, and to add walls to the generation to fit better with the domain.
	/// </summary>
	public static void ForceLivingTree(int i, int j, bool patch)
	{
		int num = 0;
		int[] array = new int[1000];
		int[] array2 = new int[1000];
		int[] array3 = new int[1000];
		int[] array4 = new int[1000];
		int num2 = 0;
		int[] array5 = new int[2000];
		int[] array6 = new int[2000];
		bool[] array7 = new bool[2000];

		int num3 = i - WorldGen.genRand.Next(2, 3);
		int num4 = i + WorldGen.genRand.Next(2, 3);

		if (WorldGen.genRand.NextBool(5))
		{
			if (WorldGen.genRand.NextBool(2))
			{
				num3--;
			}
			else
			{
				num4++;
			}
		}

		int num5 = num4 - num3;
		bool flag = num5 >= 4;
		int num6 = i - 50;
		int num7 = i + 50;

		if (patch)
		{
			num6 = i - 20;
			num7 = i + 20;
			num3 = i - WorldGen.genRand.Next(1, 3);
			num4 = i + WorldGen.genRand.Next(1, 3);
			flag = num5 >= 4;
		}

		int num8 = num3;
		int num9 = num4;
		int minl = num3;
		int minr = num4;
		bool flag2 = true;
		int num10 = WorldGen.genRand.Next(-8, -4);
		int num11 = WorldGen.genRand.Next(2);
		int num12 = j;
		int num13 = WorldGen.genRand.Next(5, 15);
		Main.tileSolid[48] = false;

		while (flag2)
		{
			num10++;
			if (num10 > num13)
			{
				num13 = WorldGen.genRand.Next(5, 15);
				num10 = 0;
				array2[num] = num12 + WorldGen.genRand.Next(5);

				if (WorldGen.genRand.NextBool(5))
				{
					num11 = (num11 == 0) ? 1 : 0;
				}

				if (num11 == 0)
				{
					array3[num] = -1;
					array[num] = num3;
					array4[num] = num4 - num3;

					if (WorldGen.genRand.NextBool(2))
					{
						num3++;
					}

					num8++;
					num11 = 1;
				}
				else
				{
					array3[num] = 1;
					array[num] = num4;
					array4[num] = num4 - num3;

					if (WorldGen.genRand.NextBool(2))
					{
						num4--;
					}

					num9--;
					num11 = 0;
				}

				if (num8 == num9)
				{
					flag2 = false;
				}

				num++;
			}

			for (int m = num3; m <= num4; m++)
			{
				PlaceLivingTile(m, num12);
			}

			num12--;
		}

		for (int n = 0; n < num - 1; n++)
		{
			int num14 = array[n] + array3[n];
			int num15 = array2[n];
			int num16 = (int)(array4[n] * (1.0 + WorldGen.genRand.Next(20, 30) * 0.1));

			PlaceLivingTile(num14, num15 + 1);

			int num17 = WorldGen.genRand.Next(3, 5);

			while (num16 > 0)
			{
				num16--;
				PlaceLivingTile(num14, num15);

				if (WorldGen.genRand.NextBool(10))
				{
					num15 = (!WorldGen.genRand.NextBool(2)) ? (num15 + 1) : (num15 - 1);
				}
				else
				{
					num14 += array3[n];
				}

				if (num17 > 0)
				{
					num17--;
				}
				else if (WorldGen.genRand.NextBool(2))
				{
					num17 = WorldGen.genRand.Next(2, 5);
					if (WorldGen.genRand.NextBool(2))
					{
						PlaceLivingTile(num14, num15);
						PlaceLivingTile(num14, num15 - 1);

						array5[num2] = num14;
						array6[num2] = num15;
						num2++;
					}
					else
					{
						PlaceLivingTile(num14, num15);
						PlaceLivingTile(num14, num15 + 1);

						array5[num2] = num14;
						array6[num2] = num15;
						num2++;
					}
				}

				if (num16 == 0)
				{
					array5[num2] = num14;
					array6[num2] = num15;
					num2++;
				}
			}
		}

		int num18 = (num3 + num4) / 2;
		int num19 = num12;
		int num20 = WorldGen.genRand.Next(num5 * 3, num5 * 5);
		int num21 = 0;
		int num22 = 0;

		while (num20 > 0)
		{
			Tile tile = Main.tile[num18, num19];
			PlaceLivingTile(num18, num19);

			if (num21 > 0)
			{
				num21--;
			}

			if (num22 > 0)
			{
				num22--;
			}

			for (int num23 = -1; num23 < 2; num23++)
			{
				if (num23 == 0 || (num23 >= 0 || num21 != 0) && (num23 <= 0 || num22 != 0) || !WorldGen.genRand.NextBool(2))
				{
					continue;
				}

				int num24 = num18;
				int num25 = num19;
				int num26 = WorldGen.genRand.Next(num5, num5 * 3);

				if (num23 < 0)
				{
					num21 = WorldGen.genRand.Next(3, 5);
				}

				if (num23 > 0)
				{
					num22 = WorldGen.genRand.Next(3, 5);
				}

				int num27 = 0;
				while (num26 > 0)
				{
					num26--;
					num24 += num23;

					PlaceLivingTile(num24, num25);

					if (num26 == 0)
					{
						array5[num2] = num24;
						array6[num2] = num25;
						array7[num2] = true;
						num2++;
					}

					if (WorldGen.genRand.NextBool(5))
					{
						num25 = (!WorldGen.genRand.NextBool(2)) ? (num25 + 1) : (num25 - 1);
						PlaceLivingTile(num24, num25);
					}

					if (num27 > 0)
					{
						num27--;
					}
					else if (WorldGen.genRand.NextBool(3))
					{
						num27 = WorldGen.genRand.Next(2, 4);
						int num28 = num24;
						int num29 = num25;
						num29 = (!WorldGen.genRand.NextBool(2)) ? (num29 + 1) : (num29 - 1);

						PlaceLivingTile(num28, num29);

						array5[num2] = num28;
						array6[num2] = num29;
						array7[num2] = true;
						num2++;
						array5[num2] = num28 + WorldGen.genRand.Next(-5, 6);
						array6[num2] = num29 + WorldGen.genRand.Next(-5, 6);
						array7[num2] = true;
						num2++;
					}
				}
			}

			array5[num2] = num18;
			array6[num2] = num19;
			num2++;

			if (WorldGen.genRand.NextBool(4))
			{
				num18 = (!WorldGen.genRand.NextBool(2)) ? (num18 + 1) : (num18 - 1);
				PlaceLivingTile(num18, num19);
			}

			num19--;
			num20--;
		}

		for (int num30 = minl; num30 <= minr; num30++)
		{
			int num31 = WorldGen.genRand.Next(1, 6);
			int num32 = j + 1;
			while (num31 > 0)
			{
				if (WorldGen.SolidTile(num30, num32))
				{
					num31--;
				}

				if (num32 >= Main.maxTilesY)
				{
					break;
				}

				PlaceLivingTile(num30, num32);
				num32++;
			}

			int num33 = num32;
			int num34 = WorldGen.genRand.Next(2, num5 + 1);
			for (int num35 = 0; num35 < num34; num35++)
			{
				num32 = num33 - 6;
				int num36 = (minl + minr) / 2;
				int num38 = 1;
				int num37 = (num30 >= num36) ? 1 : -1;

				if (num30 == num36 || num5 > 6 && (num30 == num36 - 1 || num30 == num36 + 1))
				{
					num37 = 0;
				}

				int num39 = num37;
				int num40 = num30;
				num31 = WorldGen.genRand.Next((int)(num5 * 3.5), num5 * 6);

				while (num31 > 0)
				{
					num31--;
					num40 += num37;
					if (Main.tile[num40, num32].WallType != 244)
					{
						PlaceLivingTile(num40, num32, TileID.LivingWood, WallID.LivingWoodUnsafe);
					}

					num32 += num38;
					if (Main.tile[num40, num32].WallType != 244)
					{
						PlaceLivingTile(num40, num32, TileID.LivingWood, WallID.LivingWoodUnsafe);
					}

					if (!Main.tile[num40, num32 + 1].HasTile)
					{
						num37 = 0;
						num38 = 1;
					}

					if (WorldGen.genRand.NextBool(3))
					{
						num37 = (num39 < 0) ? ((num37 == 0) ? (-1) : 0) : ((num39 <= 0) ? WorldGen.genRand.Next(-1, 2) : ((num37 == 0) ? 1 : 0));
					}

					if (WorldGen.genRand.NextBool(3))
					{
						num38 = (num38 == 0) ? 1 : 0;
					}
				}
			}
		}

		for (int num41 = 0; num41 < num2; num41++)
		{
			int num42 = WorldGen.genRand.Next(5, 8);
			num42 = (int)(num42 * (1.0 + num5 * 0.05));

			if (array7[num41])
			{
				num42 = WorldGen.genRand.Next(6, 12) + num5;
			}

			int num43 = array5[num41] - num42 * 2;
			int num44 = array5[num41] + num42 * 2;
			int num45 = array6[num41] - num42 * 2;
			int num46 = array6[num41] + num42 * 2;
			double num47 = 2.0 - WorldGen.genRand.Next(5) * 0.1;

			for (int num48 = num43; num48 <= num44; num48++)
			{
				for (int num49 = num45; num49 <= num46; num49++)
				{
					if (Main.tile[num48, num49].TileType == 191)
					{
						continue;
					}

					if (array7[num41])
					{
						if ((new Vector2D(array5[num41], array6[num41]) - new Vector2D(num48, num49)).Length() < num42 * 0.9)
						{
							PlaceLivingTile(num48, num49, TileID.LeafBlock, WallID.LivingLeaf);
						}
					}
					else if (Math.Abs(array5[num41] - num48) + Math.Abs(array6[num41] - num49) * num47 < num42)
					{
						PlaceLivingTile(num48, num49, TileID.LeafBlock, WallID.LivingLeaf);
					}
				}

				if (WorldGen.genRand.NextBool(30))
				{
					int num50 = num45;
					if (!Main.tile[num48, num50].HasTile)
					{
						for (; !Main.tile[num48, num50 + 1].HasTile && num50 < num46; num50++)
						{
						}

						if (Main.tile[num48, num50 + 1].TileType == 192)
						{
							WorldGen.PlaceTile(num48, num50, TileID.LargePiles2, mute: true, forced: false, -1, WorldGen.genRand.Next(50, 52));
						}
					}
				}

				if (array7[num41] || !WorldGen.genRand.NextBool(15))
				{
					continue;
				}

				int num51 = num46;
				int num52 = num51 + 100;

				if (Main.tile[num48, num51].HasTile)
				{
					continue;
				}

				for (; !Main.tile[num48, num51 + 1].HasTile && num51 < num52; num51++)
				{
				}

				if (Main.tile[num48, num51 + 1].TileType == 192)
				{
					continue;
				}

				if (WorldGen.genRand.NextBool(2))
				{
					WorldGen.PlaceTile(num48, num51, TileID.LargePiles2, mute: true, forced: false, -1, WorldGen.genRand.Next(47, 50));
					continue;
				}

				int num53 = WorldGen.genRand.Next(2);
				int x = 72;

				if (num53 == 1)
				{
					x = WorldGen.genRand.Next(59, 62);
				}

				WorldGen.PlaceSmallPile(num48, num51, x, num53, 185);
			}
		}

		Main.tileSolid[48] = true;
	}

	/// <summary>
	/// Used to shorten code in <see cref="ForceLivingTree(int, int, bool)"/>.
	/// </summary>
	private static void PlaceLivingTile(int i, int j, int type = TileID.LivingWood, int wallType = WallID.LivingWoodUnsafe)
	{
		if (!WorldGen.InWorld(i, j, 20))
		{
			return;
		}

		Tile tile = Main.tile[i, j];
		tile.TileType = (ushort)type;
		//tile.HasTile = true;
		tile.IsHalfBlock = false;
		tile.WallType = (ushort)wallType;
	}

	/// <summary>
	/// Nabbed from <see cref="HouseUtils.CreateBuilder(Point, StructureMap)"/>, with various safety checks removed.<br/>
	/// Same for the child methods <see cref="CreateRooms(Point)"/> and <see cref="GetRoomSolidPrecentage(Rectangle)"/>.
	/// </summary>
	public static HouseBuilder CreateHouseBuilder(Point origin, HouseType houseType)
	{
		List<Rectangle> list = CreateRooms(origin);

		if (list.Count == 0)
		{
			return HouseBuilder.Invalid;
		}

		return houseType switch
		{
			HouseType.Wood => new WoodHouseBuilder(list),
			HouseType.Desert => new DesertHouseBuilder(list),
			HouseType.Granite => new GraniteHouseBuilder(list),
			HouseType.Ice => new IceHouseBuilder(list),
			HouseType.Jungle => new JungleHouseBuilder(list),
			HouseType.Marble => new MarbleHouseBuilder(list),
			HouseType.Mushroom => new MushroomHouseBuilder(list),
			_ => new WoodHouseBuilder(list),
		};
	}

	public static List<Rectangle> CreateRooms(Point result)
	{
		Rectangle item = FindRoom(result);
		Rectangle rectangle = FindRoom(new Point(item.Center.X, item.Y + 1));
		Rectangle rectangle2 = FindRoom(new Point(item.Center.X, item.Y + item.Height + 10));
		rectangle2.Y = item.Y + item.Height - 1;
		double roomSolidPrecentage = GetRoomSolidPrecentage(rectangle);
		double roomSolidPrecentage2 = GetRoomSolidPrecentage(rectangle2);
		item.Y += 3;
		rectangle.Y += 3;
		rectangle2.Y += 3;
		var list = new List<Rectangle>();

		if (WorldGen.genRand.NextDouble() > roomSolidPrecentage + 0.2)
		{
			list.Add(rectangle);
		}

		list.Add(item);

		if (WorldGen.genRand.NextDouble() > roomSolidPrecentage2 + 0.2)
		{
			list.Add(rectangle2);
		}

		return list;
	}

	private static Rectangle FindRoom(Point origin)
	{
		bool flag = WorldUtils.Find(origin, Searches.Chain(new Searches.Left(25), new Conditions.IsSolid()), out Point result);
		bool num = WorldUtils.Find(origin, Searches.Chain(new Searches.Right(25), new Conditions.IsSolid()), out Point result2);

		if (!flag)
		{
			result = new Point(origin.X - 25, origin.Y);
		}

		if (!num)
		{
			result2 = new Point(origin.X + 25, origin.Y);
		}

		Rectangle room = new(origin.X, origin.Y, 0, 0);
		
		if (origin.X - result.X > result2.X - origin.X)
		{
			room.X = result.X;
			room.Width = Utils.Clamp(result2.X - result.X, 15, 30);
		}
		else
		{
			room.Width = Utils.Clamp(result2.X - result.X, 15, 30);
			room.X = result2.X - room.Width;
		}

		bool flag2 = WorldUtils.Find(result, Searches.Chain(new Searches.Up(10), new Conditions.IsSolid()), out Point result4);
		bool num2 = WorldUtils.Find(result2, Searches.Chain(new Searches.Up(10), new Conditions.IsSolid()), out Point result5);
		
		if (!flag2)
		{
			result4 = new Point(origin.X, origin.Y - 10);
		}

		if (!num2)
		{
			result5 = new Point(origin.X, origin.Y - 10);
		}

		room.Height = Utils.Clamp(Math.Max(origin.Y - result4.Y, origin.Y - result5.Y), 8, 12);
		room.Y -= room.Height;
		return room;
	}

	private static double GetRoomSolidPrecentage(Rectangle room)
	{
		double num = room.Width * room.Height;
		var @ref = new Ref<int>(0);
		WorldUtils.Gen(new Point(room.X, room.Y), new Shapes.Rectangle(room.Width, room.Height), Actions.Chain(new Modifiers.IsSolid(), 
			new Actions.Count(@ref)));
		return @ref.Value / num;
	}

	/// <summary>
	/// Copied from <see cref="Terraria.GameContent.Biomes.MahoganyTreeBiome.Place"/>, cleaned up and added walls.
	/// </summary>
	/// <param name="anchor"></param>
	/// <param name="top"></param>
	public static void GenerateMahoganyTree(Point anchor, Point top)
	{
		int num3 = (anchor.Y - top.Y - 9) / 5;
		int num4 = num3 * 5;
		int num5 = 0;
		double num6 = WorldGen.genRand.NextDouble() + 1.0;
		double num7 = WorldGen.genRand.NextDouble() + 2.0;

		if (WorldGen.genRand.NextBool(2))
		{
			num7 = 0.0 - num7;
		}

		for (int i = 0; i < num3; i++)
		{
			int num8 = (int)(Math.Sin((i + 1) / 12.0 * num6 * MathHelper.Pi) * num7);
			int num9 = (num8 < num5) ? (num8 - num5) : 0;

			WorldUtils.Gen(new Point(anchor.X + num5 + num9, anchor.Y - (i + 1) * 5), new Shapes.Rectangle(6 + Math.Abs(num8 - num5), 7), 
				Actions.Chain(new Modifiers.SkipTiles(21, 467, 226, 237), new Modifiers.SkipWalls(87), new SetTileNoClear(383), new Actions.PlaceWall(WallID.LivingWood), 
				new Actions.SetFrames()));
			
			WorldUtils.Gen(new Point(anchor.X + num5 + num9 + 2, anchor.Y - (i + 1) * 5), new Shapes.Rectangle(2 + Math.Abs(num8 - num5), 5), 
				Actions.Chain(new Modifiers.SkipTiles(21, 467, 226, 237), new Modifiers.SkipWalls(87), new RemoveTileAction(), 
				new Actions.PlaceWall(WallID.LivingWood)));
			
			WorldUtils.Gen(new Point(anchor.X + num5 + 2, anchor.Y - i * 5), new Shapes.Rectangle(2, 2), 
				Actions.Chain(new Modifiers.SkipTiles(21, 467, 226, 237), new Modifiers.SkipWalls(87), new RemoveTileAction(), 
				new Actions.PlaceWall(WallID.LivingWood)));
			num5 = num8;
		}

		int num10 = 6;

		if (num7 < 0.0)
		{
			num10 = 0;
		}

		var list = new List<Point>();
		for (int j = 0; j < 2; j++)
		{
			double num11 = (j + 1.0) / 3.0;
			int num12 = num10 + (int)(Math.Sin(num3 * num11 / 12.0 * num6 * MathHelper.Pi) * num7);
			double num13 = WorldGen.genRand.NextDouble() * MathHelper.PiOver4 - MathHelper.PiOver4 - 0.2;

			if (num10 == 0)
			{
				num13 -= MathHelper.PiOver2;
			}

			WorldUtils.Gen(new Point(anchor.X + num12, anchor.Y - (int)(num3 * 5 * num11)), new ShapeBranch(num13, WorldGen.genRand.Next(12, 16)).OutputEndpoints(list), 
				Actions.Chain(new Modifiers.SkipTiles(21, 467, 226, 237), new Modifiers.SkipWalls(87), new SetTileNoClear(383), new Actions.SetFrames(true),
				new Actions.PlaceWall(WallID.LivingWood)));

			num10 = 6 - num10;
		}

		int num14 = (int)(Math.Sin(num3 / 12.0 * num6 * MathHelper.Pi) * num7);
		
		WorldUtils.Gen(new Point(anchor.X + 6 + num14, anchor.Y - num4), new ShapeBranch(-0.6853981852531433, WorldGen.genRand.Next(16, 22)).OutputEndpoints(list), 
			Actions.Chain(new Modifiers.SkipTiles(21, 467, 226, 237), new Modifiers.SkipWalls(87), new SetTileNoClear(383), new Actions.SetFrames(true), 
			new Actions.PlaceWall(WallID.LivingWood)));
		
		WorldUtils.Gen(new Point(anchor.X + num14, anchor.Y - num4), new ShapeBranch(-2.45619455575943, WorldGen.genRand.Next(16, 22)).OutputEndpoints(list), 
			Actions.Chain(new Modifiers.SkipTiles(21, 467, 226, 237), new Modifiers.SkipWalls(87), new SetTileNoClear(383), new Actions.SetFrames(true),
			new Actions.PlaceWall(WallID.LivingWood)));
		
		foreach (Point item in list)
		{
			WorldUtils.Gen(item, new Shapes.Circle(4), Actions.Chain(new Modifiers.Blotches(4, 2), new Modifiers.SkipTiles(383, 21, 467, 226, 237), 
				new Modifiers.SkipWalls(78, 87), new SetTileNoClear(TileID.LivingMahoganyLeaves), new Actions.SetFrames(true), new Actions.PlaceWall(WallID.JungleUnsafe)));
		}

		for (int k = 0; k < 4; k++)
		{
			double angle = k / 3.0 * 2.0 + 0.57075;
			WorldUtils.Gen(anchor, new ShapeRoot(angle, WorldGen.genRand.Next(40, 60)), Actions.Chain(new Modifiers.SkipTiles(21, 467, 226, 237), 
				new Modifiers.SkipWalls(87), new SetTileNoClear(TileID.LivingMahogany, true), new Actions.PlaceWall(WallID.LivingWood)));
		}

		//WorldGen.AddBuriedChest(anchor.X + 3, anchor.Y - 1, (!WorldGen.genRand.NextBool(4)) ? WorldGen.GetNextJungleChestItem() : 0, notNearOtherChests: false, 10,
		//	trySlope: false, 0);
	}
}

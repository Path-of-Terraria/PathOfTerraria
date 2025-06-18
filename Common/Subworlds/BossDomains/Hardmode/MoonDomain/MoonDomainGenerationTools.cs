using ReLogic.Utilities;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonDomainGenerationTools
{
	/// <summary>
	/// <see cref="WorldGen.GrowLivingTree(int, int, bool)"/> but with the conditions removed.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <param name="patch"></param>
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
}

using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Tiles.BossDomain;
using System.Collections.Generic;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class DisableEvilOrbBossSpawning : ModSystem
{
	public static int ActualOrbsSmashed = 0;

	public override void Load()
	{
		IL_WorldGen.CheckOrb += StopBossSpawningOnOrb;
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add("orbsSmashed", (short)ActualOrbsSmashed);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		ActualOrbsSmashed = tag.GetShort("orbsSmashed");
	}

	public override void ClearWorld()
	{
		ActualOrbsSmashed = 0;
	}

	private void StopBossSpawningOnOrb(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchLdsfld<WorldGen>(nameof(WorldGen.noTileActions))))
		{
			return;
		}

		ILLabel label = null;

		if (!c.TryGotoNext(x => x.MatchBr(out label)))
		{
			return;
		}

		if (!c.TryGotoNext(MoveType.After, x => x.MatchStsfld<WorldGen>(nameof(WorldGen.shadowOrbCount))))
		{
			return;
		}

		c.Emit(OpCodes.Ldarg_0);
		c.Emit(OpCodes.Ldarg_1);
		c.EmitDelegate(ResetOrbCountIfHigh);
		c.Emit(OpCodes.Br, label);
	}

	public static void ResetOrbCountIfHigh(int i, int j)
	{
		ActualOrbsSmashed++;

		LocalizedText localizedText = (ActualOrbsSmashed % 3) switch
		{
			1 => Lang.misc[10],
			2 => Lang.misc[11],
			_ => Language.GetText("Mods.PathOfTerraria.Misc.EvilBossFailedToSummon")
		};

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			Main.NewText(localizedText.ToString(), 50, byte.MaxValue, 130);
		}
		else if (Main.netMode == NetmodeID.Server)
		{
			ChatHelper.BroadcastChatMessage(NetworkText.FromKey(localizedText.Key), new Color(50, 255, 130));
		}

		if (WorldGen.shadowOrbCount >= 3)
		{
			WorldGen.shadowOrbCount = 0;
		}

		if (ActualOrbsSmashed % 3 == 0)
		{
			SpawnChasm(i, j);
		}
	}

	private static void SpawnChasm(int i, int j)
	{
		int dir = Main.rand.NextBool() ? -1 : 1;
		int depth = Main.rand.Next(40, 50);
		float slope = Main.rand.NextFloat(-0.6f, 0.6f);
		float addY = 0;

		FastNoiseLite verticalNoise = new(Main.rand.Next());
		verticalNoise.SetFrequency(0.05f);
		verticalNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

		HashSet<Point16> tiles = [];
		HashSet<Point16> walls = [];

		for (int l = 0; l < depth; ++l)
		{
			int x = i + dir * l;
			float wallOffTop = verticalNoise.GetNoise(l, 0) * 1.2f;
			float wallOffBottom = verticalNoise.GetNoise(l, 0) * 1.2f;

			float emptyOffTop = verticalNoise.GetNoise(l, 0) * 1.2f;
			float emptyOffBottom = verticalNoise.GetNoise(l, 0) * 1.2f;

			for (int k = (int)(-8 - wallOffTop * 3); k < 8 + wallOffBottom; ++k)
			{
				int y = j + k;

				float wallStart = MathF.Min(-3 - emptyOffTop * 3, -2);
				float wallEnd = MathF.Max(3 + emptyOffBottom * 3, 2);

				if (k >= wallStart && k < wallEnd && l < depth - 4)
				{
					WorldGen.KillTile(x, y);

					walls.Add(new Point16(x, y));
				}
				else
				{
					bool isMalaise = l > depth / 2;
					int cutoffStart = depth / 2 - 4;

					if (l >= cutoffStart)
					{
						isMalaise = Main.rand.NextBool(Math.Max(5 - (l - cutoffStart), 1));
					}

					WorldGen.PlaceTile(x, y, isMalaise ? ModContent.TileType<WeakMalaise>() : TileID.Ebonstone, true, true);

					Tile tile = Main.tile[x, y];
					tile.Slope = SlopeType.Solid;

					tiles.Add(new Point16(x, y));
				}
			}

			//addY += slope;
			addY += verticalNoise.GetNoise(l, 0) * 1.2f;

			if (addY > 1)
			{
				j += (int)addY;
				addY -= (int)addY;
			}
		}

		foreach (Point16 point in tiles)
		{
			if (Main.rand.NextBool(3))
			{
				continue;
			}

			Tile.SmoothSlope(point.X, point.Y);
		}

		FastNoiseLite wallNoise = new();
		wallNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		wallNoise.SetFrequency(0.025f);
		wallNoise.SetFractalType(FastNoiseLite.FractalType.PingPong);

		foreach (Point16 point in walls)
		{
			Tile tile = Main.tile[point];
			tile.WallType = wallNoise.GetNoise(point.X, point.Y) > 0.4f ? WallID.GreenStainedGlass : WallID.EbonstoneUnsafe;
		}
	}
}

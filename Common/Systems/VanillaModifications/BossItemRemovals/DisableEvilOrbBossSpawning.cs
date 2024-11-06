using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.Chat;
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
		float addY = 0;

		FastNoiseLite noise = new(Main.rand.Next());
		noise.SetFrequency(0.05f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

		for (int l = 0; l < depth; ++l)
		{
			int x = i + dir * l;

			for (int k = -5; k < 5; ++k)
			{
				int y = j + k;

				if (k >= -2 && k < 2)
				{
					WorldGen.KillTile(x, y);
				}
				else
				{
					WorldGen.PlaceTile(x, y, l > depth / 2 ? ModContent.TileType<WeakMalaise>() : TileID.Ebonstone, true, true);
				}
			}

			float noiseLevel = noise.GetNoise(l, 0) * 5f;
			addY += MathF.Min(noiseLevel, 0.8f);

			if (addY > 1)
			{
				j += (int)addY;
				addY -= (int)addY;
			}
		}
	}
}

using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.Systems.RealtimeGen.Generation;
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
			_ => Language.GetText("Mods.PathOfTerraria.Misc.ChasmAppears")
		};

		Color color = ActualOrbsSmashed % 3 == 0 ? Color.Purple : new Color(50, 255, 130);

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			Main.NewText(localizedText.ToString(), color.R, color.G, color.B);
		}
		else if (Main.netMode == NetmodeID.Server)
		{
			ChatHelper.BroadcastChatMessage(NetworkText.FromKey(localizedText.Key), color);
		}

		if (WorldGen.shadowOrbCount >= 3)
		{
			WorldGen.shadowOrbCount = 0;
		}

		if (ActualOrbsSmashed % 3 == 0)
		{
			EoWChasmGeneration.SpawnChasm(i, j);
		}
	}
}

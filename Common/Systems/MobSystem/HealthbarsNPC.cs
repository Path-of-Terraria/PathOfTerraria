using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace PathOfTerraria.Common.Systems.MobSystem;

internal class HealthbarsNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public bool AlwaysDisplayHealthbar;

	public override void Load()
	{
		IL_Main.DrawInterface_14_EntityHealthBars += EntityHealthBarsInjection;
	}

	private static void EntityHealthBarsInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		int locNpcIndex = -1;

		// Match 'if (npc[num2].life != npc[num2].lifeMax && ...)'.
		il.GotoNext(
			MoveType.After,
			i => i.MatchLdsfld(typeof(Main), nameof(Main.npc)),
			i => i.MatchLdloc(out locNpcIndex),
			i => i.MatchLdelemRef(),
			i => i.MatchLdfld(typeof(NPC), nameof(NPC.lifeMax)),
			i => i.MatchBeq(out _)
		);

		// All we do here is cause the equality comparison to fail if we want the healthbar to render.
		il.Index--;
		il.Emit(OpCodes.Ldloc, locNpcIndex);
		il.EmitDelegate(ModifyHealthbarDisplayComparisonValue);
	}

	private static int ModifyHealthbarDisplayComparisonValue(int lifeMax, int npcIndex)
	{
		return ShouldForceNpcHealthbarDisplay(npcIndex) ? Main.npc[npcIndex].life - 1 : lifeMax;
	}

	private static bool ShouldForceNpcHealthbarDisplay(int npcIndex)
	{
		NPC npc = Main.npc[npcIndex];
		return npc.TryGetGlobalNPC(out HealthbarsNPC hNpc) && hNpc.AlwaysDisplayHealthbar;
	}
}
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class DisablePlanteraSpawning : ModSystem
{
	public override void Load()
	{
		IL_WorldGen.CheckJunglePlant += StopPlanteraFromSpawning;
	}

	private void StopPlanteraFromSpawning(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchCall<NPC>(nameof(NPC.SpawnOnPlayer))))
		{
			return;
		}

		ILLabel label = null;

		if (!c.TryGotoPrev(MoveType.After, x => x.MatchBgeUn(out label)))
		{
			return;
		}

		if (label is null)
		{
			return;
		}

		c.EmitDelegate(() => SubworldSystem.Current is not PlanteraDomain);
		c.Emit(OpCodes.Brtrue, label);
	}
}

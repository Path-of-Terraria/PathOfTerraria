using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.EventReworks.GoblinEvent;

internal class GoblinEventReworking : ModSystem
{
	public override void Load()
	{
		On_Main.StartInvasion += HijackStartInvasion;
		IL_Main.StartInvasion += ModifyInvasionParameters;
	}

	/// <summary>
	/// This forces a consistent size and activation for Goblin Army events.
	/// Basically, this removes the requirement for players to have at least 5 heart crystals in order for an event to start.
	/// </summary>
	/// <param name="il"></param>
	private void ModifyInvasionParameters(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchBlt(out _)))
		{
			return;
		}

		if (!c.TryGotoNext(x => x.MatchBlt(out _)))
		{
			return;
		}

		if (!c.TryGotoNext(MoveType.After, x => x.MatchLdloc0()))
		{
			return;
		}

		c.Emit(OpCodes.Pop);
		c.Emit(OpCodes.Ldc_I4, 10);
	}

	private void HijackStartInvasion(On_Main.orig_StartInvasion orig, int type)
	{
		if (type == InvasionID.GoblinArmy && SubworldSystem.Current is not RavencrestSubworld)
		{
			return;
		}

		orig(type);
	}
}

using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class RemovePowerCellFunctionality : ModSystem
{
	public override void Load()
	{
		IL_Player.TileInteractionsUse += StopNaturalGolemSpawn;
	}

	private void StopNaturalGolemSpawn(ILContext il)
	{
		ILCursor c = new(il);

		// Match Golem's NPC ID
		if (!c.TryGotoNext(x => x.MatchLdcI4(NPCID.Golem)))
		{
			return;
		}

		ILLabel label = null;

		// Match & get the label so we can exit early
		if (!c.TryGotoNext(MoveType.After, x => x.MatchBrtrue(out label)) || label is null)
		{
			return;
		}

		c.Emit(OpCodes.Br, label);
	}
}

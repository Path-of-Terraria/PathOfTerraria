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
		c.GotoNext(x => x.MatchLdcI4(NPCID.Golem));

		// Match & get the label so we can exit early
		ILLabel label = null;
		c.GotoNext(MoveType.After, x => x.MatchBrtrue(out label));

		c.Emit(OpCodes.Br, label);
	}
}

using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Utilities;

internal class BlockChestItemSyncing : ILoadable
{
	public static bool Blocking = false;

	public void Load(Mod mod)
	{
		IL_ItemSlot.LeftClick_ItemArray_int_int += BlockNetmodeRun;
	}

	private void BlockNetmodeRun(ILContext il)
	{
		ILCursor c = new(il);

		for (int i = 0; i < 2; ++i)
		{
			if (!c.TryGotoNext(x => x.MatchLdcI4(3)))
			{
				return;
			}
		}

		ILLabel label = null;

		if (!c.TryGotoNext(MoveType.After, x => x.MatchBneUn(out label)))
		{
			return;
		}

		c.EmitLdsfld(typeof(BlockChestItemSyncing).GetField(nameof(Blocking), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
		c.Emit(OpCodes.Brtrue, label);
	}

	public void Unload()
	{
	}
}

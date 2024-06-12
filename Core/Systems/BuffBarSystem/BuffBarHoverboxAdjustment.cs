using MonoMod.Cil;

namespace PathOfTerraria.Core.Systems.BuffBarSystem;

internal class BuffBarHoverboxAdjustment : ILoadable
{
	public void Load(Mod mod)
	{
		IL_Main.DrawBuffIcon += ModifyHoverbox;
	}

	private void ModifyHoverbox(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<Main>(nameof(Main.mouseY)))) // Fails if some other mod removes this opcode
		{
			return;
		}

		c.EmitDelegate(ModifyHitboxPosition);
	}

	public static int ModifyHitboxPosition(int y)
	{
		return y - BuffBarSystem.BuffPositionOffsetY;
	}

	public void Unload() { }
}

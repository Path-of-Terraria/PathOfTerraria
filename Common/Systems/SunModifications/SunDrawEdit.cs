using Mono.Cecil.Cil;
using MonoMod.Cil;
using SteelSeries.GameSense;

namespace PathOfTerraria.Common.Systems.SunModifications;

internal class SunDrawEdit : ILoadable
{
	public void Load(Mod mod)
	{
		IL_Main.DrawSunAndMoon += AddDrawHook;
	}

	private void AddDrawHook(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.Draw))))
		{
			return;
		}

		if (!c.TryGotoPrev(x => x.MatchLdsfld<Main>(nameof(Main.spriteBatch))))
		{
			return;
		}

		c.Emit(OpCodes.Ldloca_S, (byte)0);
		c.Emit(OpCodes.Ldloca_S, (byte)22);
		c.Emit(OpCodes.Ldloca_S, (byte)19);
		c.EmitDelegate(PreModifySunDrawing);
	}

	public static void PreModifySunDrawing(ref Texture2D texture, ref Vector2 position, ref Color color)
	{
		if (Main.gameMenu)
		{
			return;
		}

		foreach (SunEditInstance instance in ModContent.GetContent<SunEditInstance>())
		{
			instance.PreModifySunDrawing(ref texture, ref position, ref color);
		}
	}

	public void Unload()
	{
	}
}

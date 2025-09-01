using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;

namespace PathOfTerraria.Common.Systems.SurfaceBGModifications;

internal class SurfaceBackgroundPositionModifier : ILoadable
{
	private static Hook OffsetFarHook = null;
	private static Hook OffsetMiddleHook = null;
	private static FieldInfo ListFieldInfo = null;

	public void Load(Mod mod)
	{
		ListFieldInfo = typeof(SurfaceBackgroundStylesLoader).GetField("list", BindingFlags.Instance | BindingFlags.NonPublic);

		OffsetFarHook = new Hook(typeof(SurfaceBackgroundStylesLoader).GetMethod(nameof(SurfaceBackgroundStylesLoader.DrawFarTexture)), ModifyFar, true);
		OffsetMiddleHook = new Hook(typeof(SurfaceBackgroundStylesLoader).GetMethod(nameof(SurfaceBackgroundStylesLoader.DrawMiddleTexture)), ModifyMiddle, true);
	}

	public static void ModifyFar(Action<SurfaceBackgroundStylesLoader> orig, SurfaceBackgroundStylesLoader self)
	{
		ref int bgTopY = ref GetTopY(Main.instance);
		int oldY = bgTopY;

		if (!Main.gameMenu)
		{
			foreach (ModSurfaceBackgroundStyle style in GetList(self))
			{
				int slot = style.Slot;
				float alpha = Main.bgAlphaFarBackLayer[slot];

				if (alpha > 0f && style is IBackgroundModifier back)
				{
					back.ModifyPosition(true, ref bgTopY);
				}
			}
		}

		orig(self);
		bgTopY = oldY;
	}

	public static void ModifyMiddle(Action<SurfaceBackgroundStylesLoader> orig, SurfaceBackgroundStylesLoader self)
	{
		ref int bgTopY = ref GetTopY(Main.instance);
		int oldY = bgTopY;

		if (!Main.gameMenu)
		{
			foreach (ModSurfaceBackgroundStyle style in GetList(self))
			{
				int slot = style.Slot;
				float alpha = Main.bgAlphaFarBackLayer[slot];

				if (alpha > 0f && style is IBackgroundModifier back)
				{
					back.ModifyPosition(false, ref bgTopY);
				}
			}
		}

		orig(self);
		bgTopY = oldY;
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "bgTopY")]
	extern static ref int GetTopY(Main instance);

	public static List<ModSurfaceBackgroundStyle> GetList(SurfaceBackgroundStylesLoader instance)
	{
		return ListFieldInfo.GetValue(instance) as List<ModSurfaceBackgroundStyle>;
	}

	public void Unload()
	{
		OffsetFarHook?.Undo();
		OffsetMiddleHook?.Undo();

		OffsetMiddleHook = OffsetFarHook = null;
	}
}

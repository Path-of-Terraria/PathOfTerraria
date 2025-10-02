using System.Reflection;
using System.Runtime.InteropServices;
using MonoMod.Cil;

#nullable enable

namespace PathOfTerraria.Core.Fixes;

/// <summary>
/// Does the same thing as https://github.com/tModLoader/tModLoader/pull/4807
/// Remove after that hits stable.
/// </summary>
internal sealed class NfdPatch : ModSystem
{
	private static MethodInfo? freeMethod;

	public override void Load()
	{
		MethodInfo? method = typeof(nativefiledialog).GetMethod("UTF8_ToManaged", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		freeMethod = typeof(nativefiledialog).GetMethod("free", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

		// Only inject if the methods are found and 'free' is still extern.
		if (method != null && freeMethod != null && (freeMethod.MethodImplementationFlags & MethodImplAttributes.InternalCall) == 0)
		{
			MonoModHooks.Modify(method, Injection);
		}
	}

	private static void Injection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		while (il.TryGotoNext(MoveType.Before, i => i.MatchCall(freeMethod!)))
		{
			il.Remove();
			il.EmitCall(typeof(Marshal).GetMethod(nameof(Marshal.FreeHGlobal))!);
		}
	}
}

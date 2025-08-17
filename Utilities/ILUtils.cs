using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace PathOfTerraria.Utilities;

internal static class ILUtils
{
	/// <summary>
	/// When there are labels pointing to the next instruction, inserts a no-op, redirects labels to it, and steps after it.
	/// </summary>
	public static ILCursor HijackIncomingLabels(ILCursor cursor)
	{
		ILLabel[] incomingLabels = cursor.IncomingLabels.ToArray();

		cursor.Emit(OpCodes.Nop);

		foreach (ILLabel incomingLabel in incomingLabels)
		{
			incomingLabel.Target = cursor.Prev;
		}

		return cursor;
	}
}

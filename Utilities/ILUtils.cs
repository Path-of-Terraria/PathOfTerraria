using System.Linq;
using Mono.Cecil;
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

	/// <inheritdoc cref="AddLocalVariable(ILContext, Type)"/>
	public static int AddLocalVariable(ILCursor cursor, Type type)
	{
		return AddLocalVariable(cursor.Context, type);
	}

	/// <summary> Declares a new local variable in the context's body. Returns its index. </summary>
	public static int AddLocalVariable(ILContext ctx, Type type)
	{
		TypeReference importedType = ctx.Import(type);
		int localId = ctx.Body.Variables.Count;

		ctx.Body.Variables.Add(new VariableDefinition(importedType));

		return localId;
	}
}

using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PathOfTerraria.Utilities;

#nullable enable

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

	/// <summary>
	/// Creates an IL edit "detour" that, in two parts, works the same as a standard detour but doesn't bloat the call stack. 
	/// </summary>
	public static void EmitILDetour<T>(MethodInfo hook, T? before, T? after) where T : Delegate
	{
		MonoModHooks.Modify(hook, (context) =>
		{
			ILCursor c = new(context);
			int paramCount = typeof(T).GenericTypeArguments.Length;

			if (before != null)
			{
				EmitAllArguments();
				c.EmitDelegate(before);
			}

			if (after != null)
			{
				Queue<int> indexes = [];

				while (c.TryGotoNext(x => x.MatchRet()))
				{
					indexes.Enqueue(c.Index);
				}

				while (indexes.Count > 0)
				{
					c.Index = indexes.Dequeue();
					EmitAllArguments();
					c.EmitDelegate(after);
				}
			}

			void EmitAllArguments()
			{
				for (int i = 0; i < paramCount; ++i)
				{
					c.Emit(OpCodes.Ldarg_S, (byte)i);
				}
			}
		});
	}
}

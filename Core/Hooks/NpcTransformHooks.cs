using System.Diagnostics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Utilities;
using Terraria.ModLoader.Core;
using Hook = PathOfTerraria.Core.Hooks.INpcTransformCallbacks;

namespace PathOfTerraria.Core.Hooks;

#nullable enable

#pragma warning disable IDE0220 // Add explicit cast

/// <summary>
/// Various hooks relating to the <see cref="NPC.Transform"/> method.
/// </summary>
internal interface INpcTransformCallbacks
{
	public static readonly GlobalHookList<GlobalNPC> PreTransformHook = NPCLoader.AddModHook(GlobalHookList<GlobalNPC>.Create(i => ((Hook)i).PreTransform));
	public static readonly GlobalHookList<GlobalNPC> TransformTransferHook = NPCLoader.AddModHook(GlobalHookList<GlobalNPC>.Create(i => ((Hook)i).TransformTransfer));
	public static readonly GlobalHookList<GlobalNPC> PostTransformHook = NPCLoader.AddModHook(GlobalHookList<GlobalNPC>.Create(i => ((Hook)i).PostTransform));

	/// <summary>
	/// Called before an NPC transforms using <see cref="NPC.Transform"/>.
	/// </summary>
	public virtual void PreTransform(NPC npc, int oldType) { }

	/// <summary>
	/// Allows data to be transferred between global instances during <see cref="NPC.Transform"/>'s invocation.
	/// <br/> Is not called for <see cref="ModNPC"/> instances.
	/// </summary>
	public virtual void TransformTransfer(NPC npc, int oldType, Hook? oldInstance) { }

	/// <summary>
	/// Called after an NPC transforms using <see cref="NPC.Transform"/>.
	/// </summary>
	public virtual void PostTransform(NPC npc, int oldType) { }

	public static void InvokePreTransform(NPC npc, int oldType)
	{
		if (npc.ModNPC is Hook modNpc)
		{
			modNpc.PreTransform(npc, oldType);
		}

		foreach (Hook g in PreTransformHook.Enumerate(npc))
		{
			g.PreTransform(npc, oldType);
		}
	}

	public static void InvokePostTransform(NPC npc, int oldType)
	{
		if (npc.ModNPC is Hook modNpc)
		{
			modNpc.PostTransform(npc, oldType);
		}

		foreach (Hook g in PostTransformHook.Enumerate(npc))
		{
			g.PostTransform(npc, oldType);
		}
	}

	public static void InvokeTransformTransfer(NPC npc, int oldType, ReadOnlySpan<GlobalNPC> oldGlobals)
	{
#if DEBUG
		// The below lookups assume that all globals, old and current, are ordered by their StaticIndex.
		// So let us assert that in Debug mode.
		for (int i = 1; i < oldGlobals.Length; i++)
		{
			Debug.Assert(oldGlobals[i - 1].StaticIndex < oldGlobals[i].StaticIndex);
		}
#endif

		int oldIndex = 0;

		foreach (Hook newInstance in TransformTransferHook.Enumerate(npc))
		{
			var newGlobal = (GlobalNPC)newInstance;
			Hook? oldInstance = null;

			// Lookup the old instance matching this global.
			int oldIndexCopy = oldIndex;
			for (; oldIndex < oldGlobals.Length; oldIndex++)
			{
				GlobalNPC oldGlobal = oldGlobals[oldIndex];
				if (oldGlobal.StaticIndex == newGlobal.StaticIndex)
				{
					oldInstance = (Hook)oldGlobal;
					break;
				}
			}

			if (oldInstance == null)
			{
				oldIndex = oldIndexCopy;
			}

			newInstance.TransformTransfer(npc, oldType, oldInstance);
		}
	}
}

file class NpcTransformHooksImpl : ILoadable
{
	void ILoadable.Load(Mod mod)
	{
		IL_NPC.Transform += TransformInjection;
	}

	void ILoadable.Unload() { }

	private static void TransformInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Define local variables.
		int locOldGlobals = ILUtils.AddLocalVariable(ctx, typeof(GlobalNPC[]));
		int locOldType = -1;

		// Match 'int oldType = type;'.
		il.GotoNext(
			MoveType.Before,
			i => i.MatchLdarg0(),
			i => i.MatchLdfld(typeof(NPC), nameof(NPC.type)),
			i => i.MatchStloc(out locOldType)
		);
		// Emit PreTransform hook.
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldarg_1);
		il.EmitDelegate(Hook.InvokePreTransform);
		// Emit export
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldloca, locOldGlobals);
		il.EmitDelegate(ExportGlobals);

		// Match 'spriteDirection = num5;'.
		il.GotoNext(
			MoveType.After,
			i => i.MatchLdarg0(),
			i => i.MatchLdloc(out _),
			i => i.MatchStfld(typeof(NPC), nameof(NPC.spriteDirection))
		);
		// Emit import.
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldloc, locOldType);
		il.Emit(OpCodes.Ldloc, locOldGlobals);
		il.EmitDelegate(ImportGlobals);

		// Match 'altTexture = 0;'.
		il.GotoNext(
			MoveType.After,
			i => i.MatchStfld(typeof(NPC), nameof(NPC.altTexture))
		);
		// Emit PostTransform invoke.
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldloc, locOldType);
		il.EmitDelegate(Hook.InvokePostTransform);
	}

	private static void ExportGlobals(NPC npc, out GlobalNPC[] result)
	{
		int numGlobals = 0;
		EntityGlobalsEnumerator<GlobalNPC> it = npc.Globals;
		while (it.MoveNext()) { numGlobals++; }

		// If it were plausible to create a try-finally structure in the above stack, then we could fearlessly rent arrays instead.
		result = new GlobalNPC[numGlobals]; //ArrayPool<GlobalNPC>.Shared.Rent(numGlobals);

		int idx = 0;
		foreach (GlobalNPC global in npc.Globals)
		{
			result[idx++] = global;
		}
	}

	private static void ImportGlobals(NPC npc, int oldType, GlobalNPC[] array)
	{
		Hook.InvokeTransformTransfer(npc, oldType, array);
	}
}

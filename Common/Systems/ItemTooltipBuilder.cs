using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Utilities;

namespace PathOfTerraria.Common.Systems;

// This funny system functions in three *very much not simple* steps:
// - First, a method is constructed to invoke MouseText_DrawItemTooltip with ease.
// - Second, MouseText_DrawItemTooltip is modified to support an "evaluation mode", that skips all draw calls, and grabs the finalized tooltip list.
// - Third, the public-facing BuildTooltips method uses the aforementioned two elements and a HoverItem substitution to create and return tooltips for the provided item.
// -- Mirsario.

/// <summary>
/// Helper class for building an item's tooltips easily.
/// </summary>
public sealed class ItemTooltipBuilder : ModSystem
{
	private const BindingFlags AnyFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

	private static uint drawTooltipEvaluationCounter;
	private static List<DrawableTooltipLine> lastDrawTooltipsList;

	public override void Load()
	{
		MonoModHooks.Modify(GetType().GetMethod(nameof(VanillaDrawItemTooltip), AnyFlags), VanillaDrawItemTooltipCallBuilder);
		IL_Main.MouseText_DrawItemTooltip += TooltipEvaluationModeInjection;
	}

	/// <summary>
	/// Builds and returns an item's tooltips.
	/// </summary>
	/// <param name="item">Item to use.</param>
	/// <param name="player">Player who's looking at the tooltips.</param>
	/// <returns>The lines for each tooltip.</returns>
	public static List<DrawableTooltipLine> BuildTooltips(Item item, Player player)
	{
		Item hoverItemCopy = Main.HoverItem;
		Player localPlayerCopy = Main.LocalPlayer;

		try
		{
			Main.HoverItem = item;
			Main.player[Main.myPlayer] = player;
			drawTooltipEvaluationCounter = checked(drawTooltipEvaluationCounter + 1);

			VanillaDrawItemTooltip();

			List<DrawableTooltipLine> result = lastDrawTooltipsList;
			lastDrawTooltipsList = null;
			return result ?? [];
		}
		finally
		{
			drawTooltipEvaluationCounter = checked(drawTooltipEvaluationCounter - 1);
			Main.player[Main.myPlayer] = localPlayerCopy;
			Main.HoverItem = hoverItemCopy;
		}
	}

	// This function is an injection target.
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	private static void VanillaDrawItemTooltip()
	{

	}
	private static void VanillaDrawItemTooltipCallBuilder(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		FieldInfo mainInstance = typeof(Main).GetField(nameof(Main.instance), AnyFlags)!;
		Type cacheType = typeof(Main).GetNestedType("MouseTextCache", AnyFlags)!;
		int cacheLocalId = il.Body.Variables.Count;
		il.Body.Variables.Add(new VariableDefinition(ctx.Import(cacheType)));

		il.EmitLdsfld(mainInstance);
		il.EmitLdfld(typeof(Main).GetField("_mouseTextCache", AnyFlags)!);
		il.EmitStloc(cacheLocalId);

		il.EmitLdsfld(mainInstance);
		il.EmitLdloc(cacheLocalId);
		il.EmitLdloc(cacheLocalId); il.EmitLdfld(cacheType.GetField("rare")!);
		il.EmitLdloc(cacheLocalId); il.EmitLdfld(cacheType.GetField("diff")!);
		il.EmitLdloc(cacheLocalId); il.EmitLdfld(cacheType.GetField("X")!);
		il.EmitLdloc(cacheLocalId); il.EmitLdfld(cacheType.GetField("Y")!);
		il.EmitCall(typeof(Main).GetMethod("MouseText_DrawItemTooltip", AnyFlags)!);
	}

	private static bool IsTooltipDrawInEvaluationMode()
	{
		return drawTooltipEvaluationCounter != 0;
	}

	// Injects into MouseText_DrawItemTooltip, adds a "mode" to it to avoid rendering tooltips, only processing and exporting them.
	private static void TooltipEvaluationModeInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Save the tooltips list.
		int locDrawTooltips = -1;
		il.GotoNext(
			MoveType.After,
			i => i.MatchLdsfld(typeof(Main), nameof(Main.HoverItem)),
			i => i.MatchLdloc(out locDrawTooltips),
			i => i.MatchCallvirt(out _),
			i => i.MatchCall(typeof(ItemLoader).GetMethod(nameof(ItemLoader.PostDrawTooltip), AnyFlags))
		);
		il.GotoNext(MoveType.Before, i => i.MatchRet());
		il.Emit(OpCodes.Ldloc, locDrawTooltips);
		il.EmitDelegate(ExportDrawTooltipsList);

		// Insert conditional skips to all calls that may potentially render something.
		il.Index = 0;
		Action<ILCursor> substitute = null;
		MethodReference mRef = null!;
		Dictionary<string, Action<ILCursor>> methodsToSkip = new() {
			{ "Utils.DrawInvBG", null },
			{ "SpriteBatch.Draw", null },
			{ "ItemLoader.PreDrawTooltip", i => i.Emit(OpCodes.Ldc_I4_1) },
			{ "ItemLoader.PreDrawTooltipLine", i => i.Emit(OpCodes.Ldc_I4_1) },
			{ "ItemLoader.PostDrawTooltip", null },
			{ "ItemLoader.PostDrawTooltipLine", null },
			{ "ChatManager.DrawColorCodedStringWithShadow", il => il.EmitCall(typeof(Vector2).GetProperty(nameof(Vector2.Zero)).GetMethod) },
		};

		while (il.TryGotoNext(MoveType.Before,
			i => (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) && i.MatchCall(out mRef) && methodsToSkip.TryGetValue($"{mRef.DeclaringType?.Name}.{mRef.Name}", out substitute)
		))
		{
			Instruction callInstruction = il.Prev;

			ILUtils.HijackIncomingLabels(il);

			// Emit conditional call skip.
			ILLabel callLabel = il.DefineLabel();
			ILLabel skipCallLabel = il.DefineLabel();
			il.EmitDelegate(IsTooltipDrawInEvaluationMode);
			il.EmitBrfalse(callLabel);
			// Pop all the parameters loaded for the skipped call.
			for (int i = 0; i < mRef.Parameters.Count; i++)
			{
				il.EmitPop();
			}
			// Emit result substitute.
			substitute?.Invoke(il);
			// Jump over the call.
			il.EmitBr(skipCallLabel);

			// Mark call, go forward, mark call skip.
			il.MarkLabel(callLabel);
			il.Index++;
			il.MarkLabel(skipCallLabel);
		}

#if DEBUG
		MonoModHooks.DumpIL(PoTMod.Instance, ctx);
#endif
	}
	private static void ExportDrawTooltipsList(List<DrawableTooltipLine> list)
	{
		lastDrawTooltipsList = list;
	}
}

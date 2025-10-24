using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Utilities;
using System.Reflection;
using Terraria.GameContent;

namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// This IL edit stops an esoteric error from happening, where <see cref="PlayerContainerNPC"/>s throw an exception when the npc housing page is open.
/// That error occurs even though player container NPCs don't have housing, and even though the banners wouldn't be visible anyway. Weird!
/// </summary>
internal class HouseBannerHook : ILoadable
{
	public void Load(Mod mod)
	{
		IL_Main.DrawNPCHousesInWorld += IL_Main_DrawNPCHousesInWorld;
	}

	private void IL_Main_DrawNPCHousesInWorld(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Match 'NPC npc = npc[num];' to acquire locals.
		int locNpcInstance = -1;
		il.GotoNext(MoveType.After,
			i => i.MatchLdsfld(typeof(Main), nameof(Main.npc)),
			i => i.MatchLdloc(out _), // NPC index.
			i => i.MatchLdelemRef(),
			i => i.MatchStloc(out locNpcInstance)
		);

		// Match 'int headIndexSafe = TownNPCProfiles.GetHeadIndexSafe(npc);', which should be right after the first draw.
		il.GotoNext(MoveType.Before,
			i => i.MatchLdloc(locNpcInstance),
			i => i.MatchCall(typeof(TownNPCProfiles), nameof(TownNPCProfiles.GetHeadIndexSafe))
		);
		ILUtils.HijackIncomingLabels(il);

		// Emit a conditional jump over the following code, up to the end of the head draw.
		ILLabel skipHeadDrawLabel = il.DefineLabel();
		il.Emit(OpCodes.Ldloc_S, (byte)locNpcInstance);
		il.EmitDelegate(PreDrawNPCHeadIcon);
		il.Emit(OpCodes.Brfalse, skipHeadDrawLabel);

		// Go after the head draw and mark the label.
		il.GotoNext(MoveType.After, i => i.MatchLdsfld(typeof(Main), nameof(Main.spriteBatch)), i => i.MatchLdsfld(typeof(TextureAssets), nameof(TextureAssets.NpcHead)));
		il.GotoNext(MoveType.After, i => i.MatchCallOrCallvirt(typeof(SpriteBatch), nameof(SpriteBatch.Draw)));
		ILUtils.HijackIncomingLabels(il);
		il.MarkLabel(skipHeadDrawLabel);

		MonoModHooks.DumpIL(PoTMod.Instance, ctx);
	}

	public static bool PreDrawNPCHeadIcon(NPC npc)
	{
		// Stops player container NPCs from having their head drawn at all.
		if (npc.ModNPC is PlayerContainerNPC container)
		{
			return false;
		}

		return true;
	}

	public void Unload()
	{
	}
}

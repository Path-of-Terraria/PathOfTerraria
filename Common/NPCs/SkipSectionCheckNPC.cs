using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;

namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// Used to skip the section check that stops NPCs from updating in locations which are not loaded in multiplayer.<br/>
/// Use this when an NPC that *does not* interact with the world needs to update offscreen.
/// </summary>
internal class SkipSectionCheckNPC : ILoadable
{
	public static HashSet<int> SkipSectionCheck = [];

	public void Load(Mod mod)
	{
		IL_NPC.UpdateNPC_Inner += StopSectionCheck;
	}

	private void StopSectionCheck(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(MoveType.After, x => x.MatchCallvirt<WorldSections>(nameof(WorldSections.TilesLoaded))))
		{
			return;
		}

		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate(static (bool vanillaFlag, NPC currentNPC) =>
		{
			if (!SkipSectionCheck.Contains(currentNPC.type))
			{
				return vanillaFlag;
			}

			return true;
		});
	}

	public void Unload()
	{
		SkipSectionCheck.Clear();
	}
}

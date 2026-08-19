using MonoMod.Cil;
using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.DukeDomain;

/// <summary>
/// Stops Duke Fishron from enraging in the Duke Fishron domain.
/// </summary>
internal class FishronNPCEdit : ModSystem
{
	public override void Load()
	{
		IL_NPC.AI_069_DukeFishron += HijackBeachConditionForPlayer;
	}

	private void HijackBeachConditionForPlayer(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchStloc(28)))
		{
			return;
		}

		ILLabel label = null;

		if (!c.TryGotoNext(x => x.MatchBrfalse(out label)))
		{
			return;
		}

		c.EmitDelegate(IsValidPlaceForPlayer);
	}

	public static bool IsValidPlaceForPlayer(bool vanillaFlag)
	{
		return SubworldSystem.Current is not FishronDomain && vanillaFlag;
	}
}

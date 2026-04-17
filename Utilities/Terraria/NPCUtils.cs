using System.Linq;

#nullable enable

namespace PathOfTerraria.Utilities.Terraria;

public static class NPCUtil
{
	public static Entity? GetTargetEntity(this NPC npc)
	{
		if (npc.HasValidTarget)
		{
			if (npc.HasPlayerTarget) { return Main.player[npc.target]; }
			if (npc.HasNPCTarget) { return Main.npc[npc.TranslatedTargetIndex]; }
		}

		return null;
	}

	public static void KillAllWithType(ReadOnlySpan<int> types)
	{
		foreach (int type in types)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == type)
				{
					try { npc.StrikeInstantKill(); }
					catch { }
				}
			}
		}
	}
}
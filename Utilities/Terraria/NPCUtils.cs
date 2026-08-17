using System.Linq;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Utilities.Terraria;

public static class NPCUtil
{
	/// <summary>
	/// Whether <paramref name="type"/> is one of the Eater of Worlds' segments. Only the head is flagged by
	/// <see cref="NPCID.Sets.ShouldBeCountedAsBoss"/> and none of the three set <see cref="NPC.boss"/>, so segment
	/// checks have to be written out by hand.
	/// </summary>
	public static bool IsEaterOfWorldsPart(int type)
	{
		return type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail;
	}

	/// <summary>
	/// Whether <paramref name="self"/> is the last living Eater of Worlds segment, i.e. its death ends the fight.
	/// Killing a segment splits the worm into two new worms, so a single fight produces dozens of segment deaths
	/// (and the Eater domain spawns two Eaters); only the very last one should count as the boss kill.
	/// <br/>Dying segments are ignored whether or not they have already been deactivated, so this is safe to call
	/// from anywhere in the death pipeline.
	/// </summary>
	public static bool IsLastEaterOfWorldsPart(NPC self)
	{
		foreach (NPC other in Main.ActiveNPCs)
		{
			if (other.whoAmI == self.whoAmI || !IsEaterOfWorldsPart(other.type))
			{
				continue;
			}

			// A segment with no health left is dying too, possibly on this very frame, and may or may not have been
			// deactivated yet. Highest index wins that tie so exactly one of them ever reports itself as the last part.
			if (other.life > 0 || other.whoAmI > self.whoAmI)
			{
				return false;
			}
		}

		return true;
	}

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
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using Terraria.GameContent;

namespace PathOfTerraria.Common.NPCs;

/// <summary> Handles player-build overworld taverns. For the Ravencrest system, see <see cref="TavernManager"/>.</summary>
internal class WanderingTavernManager : ModSystem
{
	public override void Load()
	{
		On_TownRoomManager.CanNPCsLiveWithEachOther_NPC_NPC += Force;
	}

	private static bool Force(On_TownRoomManager.orig_CanNPCsLiveWithEachOther_NPC_NPC orig, TownRoomManager self, NPC npc1, NPC npc2)
	{
		bool value = orig(self, npc1, npc2);
		if (npc1.ModNPC is ITavernNPC)
		{
			return true;
		}

		return value;
	}
}
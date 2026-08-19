using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

internal class DisableFighterAIDoorBreaking : GlobalNPC
{
	public override void Load()
	{
		On_NPC.AI_003_Fighters += AddFighterFlagBlocking;
	}

	private static void AddFighterFlagBlocking(On_NPC.orig_AI_003_Fighters orig, NPC self)
	{
		if (SubworldSystem.Current is not RavencrestSubworld)
		{
			orig(self);
			return;
		}

		int type = self.type;
		
		// Apparently only Peons can break doors? Okay? Swap the type during this method so it doesn't break doors.
		if (self.type == NPCID.GoblinPeon)
		{
			self.type = NPCID.GoblinThief;
		}

		orig(self);
		self.type = type;
	}
}

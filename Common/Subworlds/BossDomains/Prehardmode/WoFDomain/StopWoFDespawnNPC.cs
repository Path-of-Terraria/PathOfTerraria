using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.WoFDomain;

/// <summary>
/// Stops the Wall of Flesh from despawning in the <see cref="WallOfFleshDomain"/>.<br/>
/// If it is allowed to despawn, the kill will count and the player will have succeeded even if they only died.
/// </summary>
internal class StopWoFDespawnNPC : GlobalNPC
{
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
	{
		return entity.type == NPCID.WallofFlesh;
	}

	public override bool PreAI(NPC npc)
	{
		if (SubworldSystem.Current is WallOfFleshDomain)
		{
			npc.localAI[1] = 0;
		}

		return true;
	}
}

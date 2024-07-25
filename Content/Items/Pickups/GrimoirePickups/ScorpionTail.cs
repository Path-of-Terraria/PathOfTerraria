using NPCUtils;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

internal class ScorpionTail : GrimoirePickup
{
	public override Point Size => new(16, 20);

	public override void AddDrops(NPC npc, ref NPCLoot loot)
	{
		if (npc.type == NPCID.Scorpion || npc.type == NPCID.ScorpionBlack)
		{
			loot.AddCommon<ScorpionTail>(20);
		}
	}
}
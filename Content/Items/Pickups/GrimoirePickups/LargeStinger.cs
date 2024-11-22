using NPCUtils;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

internal class LargeStinger : GrimoirePickup
{
	public override Point Size => new(28, 26);

	public override void AddDrops(NPC npc, ref NPCLoot loot)
	{
		if (npc.type == NPCID.Hornet)
		{
			loot.AddCommon<ScorpionTail>(100);
		}
	}
}
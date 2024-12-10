using NPCUtils;
using PathOfTerraria.Content.NPCs.HellEvent;

namespace PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

internal class SoulfulAsh : GrimoirePickup
{
	public override Point Size => new(20, 20);

	public override void AddDrops(NPC npc, ref NPCLoot loot)
	{
		if (npc.type == ModContent.NPCType<AshWraith>())
		{
			loot.AddCommon<SoulfulAsh>(60);
		}
	}
}
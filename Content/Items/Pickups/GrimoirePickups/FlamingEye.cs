using NPCUtils;
using PathOfTerraria.Content.NPCs.HellEvent;

namespace PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

internal class FlamingEye : GrimoirePickup
{
	public override Point Size => new(38, 38);

	public override void AddDrops(NPC npc, ref NPCLoot loot)
	{
		if (npc.type == ModContent.NPCType<AshWraith>())
		{
			loot.AddCommon<FlamingEye>(60);
		}
	}
}
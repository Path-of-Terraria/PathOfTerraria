using NPCUtils;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

internal class BatWings : GrimoirePickup
{
	public override Point Size => new(44, 40);

	public override void AddDrops(NPC npc, ref NPCLoot loot)
	{
		if (npc.type == NPCID.CaveBat)
		{
			loot.AddCommon<BatWings>(20);
		}
	}
}
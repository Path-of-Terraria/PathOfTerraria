using NPCUtils;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

internal class BatWings : GrimoirePickup
{
	public override Point Size => new(44, 40);

	public override void AddDrops(NPC npc, ref NPCLoot loot)
	{
		if (npc.type == NPCID.CaveBat || npc.type == NPCID.JungleBat)
		{
			loot.AddCommon<BatWings>(npc.type == NPCID.JungleBat ? 15 : 20);
		}
	}
}
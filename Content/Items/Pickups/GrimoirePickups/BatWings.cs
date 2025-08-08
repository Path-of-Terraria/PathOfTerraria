using NPCUtils;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

internal class BatWings : GrimoirePickup
{
	public override Point Size => new(44, 40);

	public override void AddDrops(NPC npc, ref NPCLoot loot)
	{
		switch (npc.type)
		{
			case NPCID.CaveBat:
				loot.AddCommon<BatWings>(20);
				break;
			case NPCID.JungleBat:
				loot.AddCommon<BatWings>(15);
				break;
			case NPCID.IceBat:
				loot.AddCommon<BatWings>(15);
				break;
			case NPCID.SporeBat:
				loot.AddCommon<BatWings>(15);
				break;
			case NPCID.Hellbat:
				loot.AddCommon<BatWings>(10);
				break;
		}
	}
}
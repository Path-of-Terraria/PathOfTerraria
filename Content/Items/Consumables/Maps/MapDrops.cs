using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal class MapDrops : GlobalNPC
{
	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
		if (npc.type == NPCID.KingSlime) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowTierMap>()));
		}
		
		if (NPC.downedSlimeKing) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowTierMap>(), 300));
		}
	}
}
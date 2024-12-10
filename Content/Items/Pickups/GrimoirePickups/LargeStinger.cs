using PathOfTerraria.Common.NPCs;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

internal class LargeStinger : GrimoirePickup
{
	public override Point Size => new(28, 26);

	public override void AddDrops(NPC npc, ref NPCLoot loot)
	{
		if (npc.type == NPCID.Hornet)
		{
			BossDownedCondition.Bosses boss = BossDownedCondition.Bosses.QueenBee;
			loot.Add(ItemDropRule.ByCondition(new BossDownedCondition(boss), ModContent.ItemType<LargeStinger>(), 100));
		}
	}
}
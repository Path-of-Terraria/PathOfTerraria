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
			loot.Add(ItemDropRule.ByCondition(new DownedQueenBee(), ModContent.ItemType<LargeStinger>(), 100));
		}
	}

	public class DownedQueenBee : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			return NPC.downedQueenBee;
		}

		public bool CanShowItemDropInUI()
		{
			return true;
		}

		public string GetConditionDescription()
		{
			return null;
		}
	}
}
using PathOfTerraria.Content.Items.Quest;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.NPCs.ConditionalDropping;

internal class ConditionalDropHandler : GlobalNPC
{
	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		if (npc.type is NPCID.GoblinScout)
		{
			AddCountCondition(npcLoot, ModContent.ItemType<TomeOfTheElders>(), 1);
		}
		else if (NPCID.Sets.Zombies[npc.type] || NPCID.Sets.DemonEyes[npc.type])
		{
			AddCountCondition(npcLoot, ModContent.ItemType<LunarShard>(), 2);
		}
		else if (npc.type is NPCID.WanderingEye or NPCID.PossessedArmor or NPCID.Wraith)
		{
			AddCountCondition(npcLoot, ModContent.ItemType<LunarFragment>(), 2);
		}
	}

	private static void AddCountCondition(NPCLoot npcLoot, int itemId, int denominator)
	{
		npcLoot.Add(ItemDropRule.ByCondition(new PlayerCountCondition(LocalizedText.Empty, itemId), itemId, denominator));
	}

	public class PlayerCountCondition(LocalizedText conditionName, int itemId) : IItemDropRuleCondition
	{
		private readonly LocalizedText _conditionName = conditionName;
		private readonly int _itemId = itemId;

		public bool CanDrop(DropAttemptInfo info)
		{
			return info.player.GetModPlayer<ConditionalDropPlayer>().TrackedIds.TryGetValue(_itemId, out int count) && count > 0;
		}

		public bool CanShowItemDropInUI()
		{
			return true;
		}

		public string GetConditionDescription()
		{
			return _conditionName.Value;
		}
	}
}

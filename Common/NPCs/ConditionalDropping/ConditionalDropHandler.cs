using PathOfTerraria.Content.Items.Quest;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.NPCs.ConditionalDropping;

internal class ConditionalDropHandler : GlobalNPC
{
	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		if (npc.type is NPCID.GoblinArcher or NPCID.GoblinPeon or NPCID.GoblinScout or
			NPCID.GoblinSorcerer or NPCID.GoblinThief or NPCID.GoblinWarrior)
		{
			AddCountCondition(npcLoot, LocalizedText.Empty, ModContent.ItemType<TomeOfTheElders>(), 8);
		}
		else if (NPCID.Sets.Zombies[npc.type] || NPCID.Sets.DemonEyes[npc.type])
		{
			AddCountCondition(npcLoot, LocalizedText.Empty, ModContent.ItemType<LunarShard>(), 2);
		}
	}

	private static void AddCountCondition(NPCLoot npcLoot, LocalizedText conditionName, int itemId, int denominator)
	{
		npcLoot.Add(ItemDropRule.ByCondition(new PlayerCountCondition(conditionName, itemId), itemId, denominator));
	}

	public class PlayerCountCondition : IItemDropRuleCondition
	{
		private readonly LocalizedText _conditionName;
		private readonly int _itemId;

		public PlayerCountCondition(LocalizedText conditionName, int itemId)
		{
			_conditionName = conditionName;
			_itemId = itemId;
		}

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
			return _conditionName.ToString();
		}
	}
}

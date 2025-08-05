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
		// Handle goblin NPCs
		if (npc.type is NPCID.GoblinArcher or NPCID.GoblinPeon or NPCID.GoblinScout or
			NPCID.GoblinSorcerer or NPCID.GoblinThief or NPCID.GoblinWarrior)
		{
			AddCountCondition(npcLoot, "Tome Drop", ModContent.ItemType<TomeOfTheElders>(), 8);
		}
		// Handle zombies and demon eyes
		else if (NPCID.Sets.Zombies[npc.type] || NPCID.Sets.DemonEyes[npc.type])
		{
			AddCountCondition(npcLoot, "Lunar Shard Drop", ModContent.ItemType<LunarShard>(), 2);
		}
	}

	private static void AddCountCondition(NPCLoot npcLoot, string conditionName, int itemId, int denominator)
	{
		// Debug log to verify item ID and condition
		Main.NewText($"Adding drop rule for item {itemId} with 1/{denominator} chance");
		npcLoot.Add(ItemDropRule.ByCondition(new PlayerCountCondition(conditionName, itemId), itemId, denominator));
	}

	public class PlayerCountCondition : IItemDropRuleCondition
	{
		private readonly string _conditionName;
		private readonly int _itemId;

		public PlayerCountCondition(string conditionName, int itemId)
		{
			_conditionName = conditionName;
			_itemId = itemId;
		}

		public bool CanDrop(DropAttemptInfo info)
		{
			bool canDrop = info.player.GetModPlayer<ConditionalDropPlayer>().TrackedIds.TryGetValue(_itemId, out int count) && count > 0;
			HashSet<int> trackedIds = info.player.GetModPlayer<ConditionalDropPlayer>().TrackedIds;
			return canDrop;
		}

		public bool CanShowItemDropInUI()
		{
			return true;
		}

		public string GetConditionDescription()
		{
			return _conditionName;
		}
	}
}

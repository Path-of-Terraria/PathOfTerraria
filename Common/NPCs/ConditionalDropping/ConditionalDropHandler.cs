using PathOfTerraria.Content.Items.Quest;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.NPCs.ConditionalDropping;

internal class ConditionalDropHandler : GlobalNPC
{
	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		if (npc.type is NPCID.GoblinArcher or NPCID.GoblinPeon or NPCID.GoblinScout or NPCID.GoblinSorcerer or NPCID.GoblinThief or NPCID.GoblinWarrior)
		{
			AddCountCondition(npcLoot, LocalizedText.Empty, ModContent.ItemType<TomeOfTheElders>(), 8);
		}
		else if (npc.type is NPCID.Zombie or NPCID.DemonEye || NPCID.Sets.Zombies[npc.type] || NPCID.Sets.DemonEyes[npc.type])
		{
			AddCountCondition(npcLoot, LocalizedText.Empty, ModContent.ItemType<LunarShard>(), 2);
		}
	}

	private static void AddCountCondition(NPCLoot npcLoot, LocalizedText text, int id, int denominator)
	{
		npcLoot.Add(ItemDropRule.ByCondition(new PlayerCountCondition(text, id), id, denominator));
	}

	public class PlayerCountCondition(LocalizedText text, int id) : IItemDropRuleCondition
	{
		private readonly LocalizedText _text = text;
		private readonly int _id = id;

		public bool CanDrop(DropAttemptInfo info)
		{
			return info.player.GetModPlayer<ConditionalDropPlayer>().TrackedIds.TryGetValue(_id, out int count) && count > 0;
		}

		public bool CanShowItemDropInUI()
		{
			return true;
		}

		public string GetConditionDescription()
		{
			return _text.Value;
		}
	}
}

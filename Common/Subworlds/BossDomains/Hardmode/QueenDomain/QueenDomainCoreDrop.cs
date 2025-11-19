using PathOfTerraria.Common.NPCs.DropRules;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;
using PathOfTerraria.Content.Items.Quest;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.QueenDomain;

internal class QueenDomainCoreDrop : GlobalNPC
{
	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		if (npc.type == NPCID.QueenSlimeBoss)
		{
			LeadingConditionRule rule = new(new HasQuest(ModContent.GetInstance<QueenSlimeQuest>().FullName));
			rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RoyalJellyCore>()));
			npcLoot.Add(rule);
		}
	}
}
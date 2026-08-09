using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;
using PathOfTerraria.Content.Items.Quest;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.QueenDomain;

internal class QueenDomainCoreDrop : GlobalNPC
{
	public override void OnKill(NPC npc)
	{
		if (npc.type == NPCID.QueenSlimeBoss)
		{
			string questName = ModContent.GetInstance<QueenSlimeQuest>().FullName;
			int itemType = ModContent.ItemType<RoyalJellyCore>();

			// BossTracker advances this quest for every active quest owner, not only players recorded as interacting.
			InstancedItemDrop.DropForEachEligiblePlayer(npc, itemType,
				player => Quest.PlayerHasQuest(player.whoAmI, questName) && !player.HasItem(itemType),
				interactionRequired: false);
		}
	}
}

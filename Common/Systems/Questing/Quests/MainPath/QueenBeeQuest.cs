using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class QueenBeeQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<WitchNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 7500, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new CollectCount(ItemID.Stinger, 5),
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.GotStingers"), onSuccess: 
				npc => Item.NewItem(new EntitySource_Gift(npc), npc.Bottom, ModContent.ItemType<LargeStinger>())),
			new InteractWithNPC(ModContent.NPCType<LloydNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.Complete"))
			{
				CountsAsCompletedOnMarker = true
			},
		];
	}
}
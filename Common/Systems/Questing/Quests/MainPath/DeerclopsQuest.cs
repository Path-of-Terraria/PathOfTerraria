using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using Terraria.DataStructures;
using Terraria.Localization;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class DeerclopsQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<RhineNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 7500, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new CollectCount(ModContent.GetInstance<Antlers>(), 1),
			new InteractWithNPC(ModContent.NPCType<RhineNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.RhineNPC.Dialogue.Antlers"),
				[new GiveItem(1, ModContent.ItemType<Antlers>())], true),
			new InteractWithNPC(ModContent.NPCType<HunterNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.HunterNPC.Dialogue.Deerclops"),
				onSuccess: npc => Item.NewItem(new EntitySource_Gift(npc), npc.Bottom, ModContent.ItemType<DeerclopsMap>())),
			new ConditionCheck((_) => NPC.downedDeerclops, 1, this.GetLocalization("KillDeerclops")),
			new InteractWithNPC(ModContent.NPCType<RhineNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.RhineNPC.Dialogue.Success"))
			{
				CountsAsCompletedOnMarker = true
			},
		];
	}
}
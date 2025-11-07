using PathOfTerraria.Common.Quests;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;
using PathOfTerraria.Content.NPCs.Town;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class QueenBeeQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<MorganaNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 7500, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new CollectCount("Collect", ItemID.Stinger, 5),
			new InteractWithNPC("Talk", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.QueenBeeQuest"),
				Language.GetText("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.GotStingers"), onSuccess: 
				npc =>
				{
					for (int i = 0; i < 3; ++i)
					{
						int item = Item.NewItem(new EntitySource_Gift(npc), npc.Bottom, ModContent.ItemType<LargeStinger>());

						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
						}
					}
				}),
			new ConditionCheck("Kill", _ => BossTracker.DownedInDomain<QueenBeeDomain>(NPCID.QueenBee), 1, this.GetLocalization("KillQueen"))
			{
				SkipCheck = QuestUtils.BossSkipCheck(NPCID.QueenBee)
			},
			new InteractWithNPC("Finish", NPCQuestGiver, LocalizedText.Empty, Language.GetText("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.QueenBeeKilled"))
			{
				CountsAsCompletedOnMarker = true
			},
		];
	}

	public override bool Available()
	{
		return NPC.downedBoss1;
	}

	public override string MarkerLocation()
	{
		return "Ravencrest";
	}
}
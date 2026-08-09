using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Quests;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class KingSlimeQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<GarrickNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 500,
			"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];

	public override List<QuestStep> SetSteps()
	{
		return 
		[
			new InteractWithNPC("Start", ModContent.NPCType<GarrickNPC>(), 
				Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.QuestResponse"),
				Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.QuestResponse"),
				[
					new GiveItem(5, ItemID.IronBar, ItemID.LeadBar),
					new GiveItem(5, ItemID.GoldBar, ItemID.PlatinumBar),
					new GiveItem(1, ItemID.Ruby),
				], true),
			//Give the map device to the player once req mats are given
			new ActionStep((player, _) => 
			{
				QuestUtils.SpawnNPCQuestRewardItem(player, ModContent.NPCType<GarrickNPC>(), ModContent.ItemType<Content.Items.Placeable.MapDevice>());

				return true;
			}),
			//Give the map as well
			new ActionStep((player, _) => 
			{
				QuestUtils.SpawnNPCQuestRewardItem(player, ModContent.NPCType<GarrickNPC>(), ModContent.ItemType<KingSlimeMap>());

				return true;
			}),

			new ConditionCheck("Enter", _ => SubworldSystem.Current is KingSlimeDomain, 1, this.GetLocalization("EnterDomain")),
			new ConditionCheck("Kill", _ => BossTracker.DownedInDomain<KingSlimeDomain>(NPCID.KingSlime), 1, this.GetLocalization("Kill.KingSlime"))
			{
				SkipCheck = QuestUtils.BossSkipCheck(NPCID.KingSlime)
			},
			new InteractWithNPC("Finish", ModContent.NPCType<GarrickNPC>(), LocalizedText.Empty, this.GetLocalization("ThanksDialogue")) { CountsAsCompletedOnMarker = true }
		];
	}

	protected override bool InternalAvailable()
	{
		Quest[] checks = 
		[
			GetLocalPlayerInstance<BlacksmithStartQuest>(),
			GetLocalPlayerInstance<WizardStartQuest>(),
			GetLocalPlayerInstance<WitchStartQuest>(),
			GetLocalPlayerInstance<HunterStartQuest>()
		];

		return checks.Any(x => x.Completed);
	}

	public override string MarkerLocation()
	{
		return "Ravencrest";
	}
}
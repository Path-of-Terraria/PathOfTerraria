using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using Terraria.ID;
using Terraria.Localization;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Common.NPCs.ConditionalDropping;
using PathOfTerraria.Common.Subworlds;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal class TwinsQuest() : Quest
{
	public static uint? CompletionVisit { get; private set; }

	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<TinkerNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
		{
			p.GetModPlayer<ExpModPlayer>().Exp += 30000;
			CompletionVisit = (uint)MappingWorld.GetTimesEntered<RavencrestSubworld>();
		}, "30000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new InteractWithNPC("Start", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue1"), 
					Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue1")),

			new ActionStep((_, _) =>
			{
				Main.LocalPlayer.GetModPlayer<ConditionalDropPlayer>().AddId<LunarFragment>();
				return true;
			}),

			new ParallelQuestStep("Branch", [
				new CollectCount("Collect", ModContent.ItemType<LunarFragment>(), 5),
				new KillCount("Wraith", NPCID.Wraith, 3, this.GetLocalization("Wraiths")),
				new KillCount("Armor", NPCID.PossessedArmor, 3, this.GetLocalization("PossessedArmors")),
				new KillCount("Eye", NPCID.WanderingEye, 3, this.GetLocalization("WanderingEyes")),
			], this.GetLocalization("CollectFragments")),

			new ActionStep((_, _) =>
			{
				Main.LocalPlayer.GetModPlayer<ConditionalDropPlayer>().RemoveId<LunarFragment>();
				return true;
			}),

			new InteractWithNPC("Talk", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue2"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue2"),
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<TwinsMap>())), //TODO: THIS WILL BE SOME TELEPORTER FEATURE IN THE FUTURE
			
			new ConditionCheck("Domain", _ => SubworldSystem.Current is TwinsDomain, 1, this.GetLocalization("EnterDomain"))
			{
				RecoveryItem = ModContent.ItemType<TwinsMap>()
			},

			new ParallelQuestStep("Kill", [
				new ConditionCheck("Retinazer", _ => BossTracker.DownedInDomain<TwinsDomain>(NPCID.Retinazer), 1, this.GetLocalization("Boss1")),
				new ConditionCheck("Spazmatism", _ => BossTracker.DownedInDomain<TwinsDomain>(NPCID.Spazmatism), 1, this.GetLocalization("Boss2")),
			], this.GetLocalization("EnterDomain"))
			{
				RecoveryItem = ModContent.ItemType<TwinsMap>()
			},

			new InteractWithNPC("Finish", NPCQuestGiver, this.GetLocalization("Boss1"), Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue3"))
			{
				CountsAsCompletedOnMarker = true
			}
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	protected override bool InternalAvailable()
	{
		Quest tinkerIntroQuest = GetLocalPlayerInstance<TinkerIntroQuest>();
		RavencrestSubworld subworld = ModContent.GetInstance<RavencrestSubworld>();
		return tinkerIntroQuest.Completed && NPC.downedQueenSlime && MappingWorld.GetTimesEntered<RavencrestSubworld>() != TinkerIntroQuest.CompletionVisit;
	}
}

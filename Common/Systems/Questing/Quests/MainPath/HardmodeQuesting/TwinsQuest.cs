using PathOfTerraria.Common.Subworlds;
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

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal class TwinsQuest() : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<TinkerNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
		{
			p.GetModPlayer<ExpModPlayer>().Exp += 30000;
		}, "30000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue1"), 
					Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue1")),
				
			new ParallelQuestStep([
				new CollectCount(ItemID.LunarOre, 5), // Using Lunar Ore as placeholder for now. TODO: <
				new KillCount(NPCID.Wraith, 3, this.GetLocalization("Wraiths")),
				new KillCount(NPCID.PossessedArmor, 3, this.GetLocalization("PossessedArmors")),
				new KillCount(NPCID.WanderingEye, 3, this.GetLocalization("WanderingEyes")),
			], this.GetLocalization("CollectFragments")),
			
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue2"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue2"),
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<TwinsMap>())), //TODO: THIS WILL BE SOME TELEPORTER FEATURE IN THE FUTURE
			
			new ConditionCheck(_ => SubworldSystem.Current is TwinsDomain, 1, this.GetLocalization("EnterDomain")),
			
			new ParallelQuestStep([ 
				new ConditionCheck(_ => BossTracker.DownedInDomain<TwinsDomain>(NPCID.Retinazer), 1, this.GetLocalization("Boss1")),
				new ConditionCheck(_ => BossTracker.DownedInDomain<TwinsDomain>(NPCID.Spazmatism), 1, this.GetLocalization("Boss2")),
			]),
	
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue3"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerTwinsDialogue3"))
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		Quest tinkerIntroQuest = GetLocalPlayerInstance<TinkerIntroQuest>();
		return tinkerIntroQuest.Completed && NPC.downedQueenSlime;
	}
}

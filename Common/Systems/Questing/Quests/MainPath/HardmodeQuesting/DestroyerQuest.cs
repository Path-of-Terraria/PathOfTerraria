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

internal class DestroyerQuest() : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<TinkerNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
		{
			p.GetModPlayer<ExpModPlayer>().Exp += 30000;
		},
			"30000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new InteractWithNPC("Start", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerDestroyerDialogue1"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerDestroyerDialogue1")),

			new ParallelQuestStep("Branch", [
				new CollectCount("GetFlames", ItemID.CursedFlame, 20),
				new KillCount("Kill", NPCID.Clinger, 5, this.GetLocalization("WorldFeeders"))
			], Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerDestroyerDialogue1")),
			
			// TODO: Add in wavelength matching minigame? Placing antennae in overworld?

			new InteractWithNPC("Talk", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerDestroyerDialogue2"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerDestroyerDialogue2"),
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<DestroyerMap>())), //TODO: THIS WILL BE SOME TELEPORTER FEATURE IN THE FUTURE
			
			new ConditionCheck("Domain", _ => SubworldSystem.Current is DestroyerDomain, 1, this.GetLocalization("EnterDomain")),
			
			new ConditionCheck("Boss", _ => BossTracker.DownedInDomain<DestroyerDomain>(NPCID.TheDestroyer), 1, this.GetLocalization("Boss")),
	
			new InteractWithNPC("Finish", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerDestroyerDialogue3"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerDestroyerDialogue3"))
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		Quest twinsQuest = GetLocalPlayerInstance<TwinsQuest>();
		return twinsQuest.Completed && NPC.downedMechBoss2;
	}
}

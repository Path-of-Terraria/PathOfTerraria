using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.NPCs.Town;
using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds;
using Terraria.ID;
using Terraria.Localization;
using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal class EpilogueQuest() : Quest
{	
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct3;
	public override int NPCQuestGiver => ModContent.NPCType<AzarielNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 100000, "100000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new ConditionCheck("Start", plr => BossTracker.TotalBossesDowned.Contains(ModContent.NPCType<SunDevourerNPC>()) 
				|| BossTracker.TotalBossesDowned.Contains(ModContent.NPCType<Grovetender>()) || BossTracker.TotalBossesDowned.Contains(ModContent.NPCType<SunDevourerNPC>()), 0, 
				this.GetLocalization("BeatABoss"), Language.GetText("Mods.PathOfTerraria.NPCs.AzarielNPC.Dialogue.Epilogue.0")),
			
			new InteractWithNPC("Talk", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.AzarielNPC.Dialogue.EpilogueDialoue2"),
				Language.GetText("Mods.PathOfTerraria.NPCs.AzarielNPC.Dialogue.EpilogueDialogue2")),
			
			//TODO: This needs to be 100 of ANY of the elemental currencies from the Conflux league.
			new CollectCount("Finish?", ItemID.FragmentNebula, 100)
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	protected override bool InternalAvailable()
	{
		Quest moonlordQuest = GetLocalPlayerInstance<CultistMoonlordQuest>();
		return moonlordQuest.Completed && NPC.downedMoonlord;
	}
}

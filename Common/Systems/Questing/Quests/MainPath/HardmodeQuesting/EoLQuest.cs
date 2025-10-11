using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal class EoLQuest() : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<WizardNPC>();

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
			// TODO: Change this to be new prismatic fragment item 
			new CollectCount(ItemID.CrystalBall, 10),
		
			new InteractWithNPC(NPCQuestGiver,Language.GetText("Mods.PathOfTerraria.NPCs.WizardNPC.Dialogue.WizardEmpressDialogue2"),Language.GetText("Mods.PathOfTerraria.NPCs.AlaricNPC.Dialogue.WizardEmpressDialogue2")),
		
			// TODO: Make rose colored glasses item.
			new ParallelQuestStep([
				new CollectCount(ItemID.IronHelmet, 1),
				new ConditionCheck(_ => Main.LocalPlayer.armor[0].type == ItemID.IronHelmet, 1, this.GetLocalization("EquipGlasses"))
			], this.GetLocalization("EquipGlasses")),
		
			// Explore the Hallow
			new ConditionCheck(_ => Main.LocalPlayer.ZoneHallow, 1, this.GetLocalization("ExploreHallow")),
		
			// Enter the rift (Enter the Empress domain)
			new ConditionCheck(_ => SubworldSystem.Current is EmpressDomain, 1, this.GetLocalization("EnterDomain")),
		
			new ConditionCheck(_ => BossTracker.DownedInDomain<EmpressDomain>(NPCID.HallowBoss), 1, this.GetLocalization("Boss")),
		
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.WizardNPC.Dialogue.WizardEmpressDialogue3"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.WizardNPC.Dialogue.WizardEmpressDialogue3"))
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		Quest fishronQuest = GetLocalPlayerInstance<FishronQuest>();
		return fishronQuest.Completed && NPC.downedFishron;
	}
}

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
using PathOfTerraria.Content.Items.BossDomain;

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
		}, "30000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new CollectCount("Start", ModContent.ItemType<PrismShardItem>(), 10, Language.GetText("Mods.PathOfTerraria.NPCs.WizardNPC.Dialogue.EoL.0")),
		
			new InteractWithNPC("Talk", NPCQuestGiver, LocalizedText.Empty,
				Language.GetText("Mods.PathOfTerraria.NPCs.WizardNPC.Dialogue.EoL.1"), [new GiveItem(10, ModContent.ItemType<PrismShardItem>())], true,
				_ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<EoLMap>())),
		
			// Enter the rift (Enter the Empress domain)
			new ConditionCheck("Domain", _ => SubworldSystem.Current is EmpressDomain, 1, this.GetLocalization("EnterDomain")),
		
			new ConditionCheck("Boss", _ => BossTracker.DownedInDomain<EmpressDomain>(NPCID.HallowBoss), 1, this.GetLocalization("Boss")),
		
			new InteractWithNPC("Finish", NPCQuestGiver, this.GetLocalization("Boss"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.WizardNPC.Dialogue.EoL.2"))
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	protected override bool InternalAvailable()
	{
		Quest fishronQuest = GetLocalPlayerInstance<FishronQuest>();
		return fishronQuest.Completed && NPC.downedFishron;
	}
}

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

internal class SkelePrimeQuest() : Quest
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
			new InteractWithNPC("Start", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerSkeletronPrimeDialogue1"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerSkeletronPrimeDialogue1"),
				[
					new GiveItem(10, ItemID.SoulofNight),
					new GiveItem(20, ItemID.Bone),
					new GiveItem(5, ItemID.Wood), //TODO: Replace this with a new item dropped from a "mech" monster that Pyra spawns in Ravencrest
				]),
			
			new InteractWithNPC("Talk", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerSkeletronPrimeDialogue2"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerSkeletronPrimeDialogue2"),
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<PrimeMap>())), //TODO: THIS WILL BE SOME TELEPORTER FEATURE IN THE FUTURE
			
			new ConditionCheck("Domain", _ => SubworldSystem.Current is PrimeDomain, 1, this.GetLocalization("EnterDomain")),
			
			new ConditionCheck("Boss", _ => BossTracker.DownedInDomain<PrimeDomain>(NPCID.SkeletronPrime), 1, this.GetLocalization("Boss")),
	
			new InteractWithNPC("Finish", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerSkeletronPrimeDialogue3"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerSkeletronPrimeDialogue3"))
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		Quest destroyerQuest = GetLocalPlayerInstance<DestroyerQuest>();
		return destroyerQuest.Completed && NPC.downedMechBoss1;
	}
}

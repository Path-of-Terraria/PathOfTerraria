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
using PathOfTerraria.Content.Items.Quest;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal class QueenSlimeQuest() : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<GarrickNPC>();

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
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("TalkToGarrick"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.QueenSlime1")),
			
			new ParallelQuestStep([
				new CollectCount(ItemID.SoulofLight, 5),
				new KillCount(NPCID.IlluminantSlime, 10, this.GetLocalization("IlluminantSlimes"))
			], Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.QueenSlime1")),

			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("TalkToGarrick"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.QueenSlime2"),
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<QueenSlimeMap>())),

			new ConditionCheck(_ => SubworldSystem.Current is QueenSlimeDomain, 1, this.GetLocalization("EnterDomain")),
			new ConditionCheck(_ => BossTracker.DownedInDomain<QueenSlimeDomain>(NPCID.QueenSlimeBoss), 1, this.GetLocalization("Boss")),
			
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("TalkToGarrick"), Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.QueenSlime3"), [new GiveItem(1, ModContent.ItemType<RoyalJellyCore>())], true),

		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		return Main.hardMode;
	}
}

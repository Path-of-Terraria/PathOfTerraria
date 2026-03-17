using PathOfTerraria.Common.Quests;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class SkeletronQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => NPCID.OldMan;

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 10000, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		LocalizedText oldManIntro = Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.OldManDialogue");

		return
		[
			new InteractWithNPC("Start", ModContent.NPCType<BlacksmithNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.OldMan.Dialogue.QuestStart"), oldManIntro),
			new InteractWithNPC("HasItems", NPCQuestGiver, oldManIntro, Language.GetText("Mods.PathOfTerraria.NPCs.OldMan.Dialogue.HasItems"),
				[new GiveItem(2, ItemID.Candle, ItemID.PlatinumCandle), new GiveItem(1, ItemID.CrimtaneBar, ItemID.DemoniteBar),
					new GiveItem(1, ModContent.ItemType<AncientEvilBook>())]),
			new ConditionCheck("Enter", (_) => SubworldSystem.Current is SkeletronDomain, 1, this.GetLocalization("EnterDomain")),
			new ConditionCheck("Kill", _ => BossTracker.DownedInDomain<SkeletronDomain>(NPCID.SkeletronHead), 1, this.GetLocalization("KillSkeletron"))
			{
				SkipCheck = QuestUtils.BossSkipCheck(NPCID.SkeletronHead)
			},
			new InteractWithNPC("Finish", NPCID.Clothier, LocalizedText.Empty, Language.GetText("Mods.PathOfTerraria.NPCs.OldMan.Dialogue.Complete"))
			{
				CountsAsCompletedOnMarker = true
			},
		];
	}

	protected override bool InternalAvailable()
	{
		return NPC.downedBoss1;
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}
}
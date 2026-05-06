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

internal class PlanteraQuest() : HardmodeQuest(5)
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<MorganaNPC>();

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
			new ParallelQuestStep("Start", [
				new InteractWithNPC("GiveItems", NPCQuestGiver, LocalizedText.Empty, 
					Language.GetText("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.Plantera.0"),
					[
						new GiveItem(5, ItemID.Vine), // TODO: Replace this with Plant Growth item from post-mech jg mob.
						new GiveItem(10, ItemID.JungleSpores), 
					]),
				new KillCount("Kill", NPCID.JungleBat, 10, this.GetLocalization("Plantera")) //TODO: Change this to the Plant growth item mob from above^^
			], LocalizedText.Empty),

			// TODO: Add a step here that has something to do with the plant growths. Likely something that will lead into a portal spawning for plantera. 
			new InteractWithNPC("Talk", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.Plantera.0"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.Plantera.1"),
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<PlanteraMap>())),
			
			new ConditionCheck("Domain", _ => SubworldSystem.Current is PlanteraDomain, 1, this.GetLocalization("EnterDomain"))
			{
				RecoveryItem = ModContent.ItemType<PlanteraMap>()
			},

			new ConditionCheck("Boss", _ => BossTracker.DownedInDomain<PlanteraDomain>(NPCID.Plantera), 1, this.GetLocalization("Boss"))
			{
				RecoveryItem = ModContent.ItemType<PlanteraMap>()
			},

			new InteractWithNPC("Finish", NPCQuestGiver,this.GetLocalization("Boss"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.Plantera.2"))
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
		Quest skelePrimeQuest = GetLocalPlayerInstance<SkelePrimeQuest>();
		return skelePrimeQuest.Completed && NPC.downedMechBoss3;
	}
}

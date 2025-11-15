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

internal class CultistMoonlordQuest() : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<AzarielNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
		{
			p.GetModPlayer<ExpModPlayer>().Exp += 100000;
			//TODO: Start the epilogue quest right after killing moonlord.
		},
			"100000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			// TODO: Change this to be the 5 new sigils found in the respective biomes.
			new ParallelQuestStep("Start", [
				new CollectCount("Sigil1", ItemID.CelestialSigil, 1),
				new CollectCount("Sigil2", ItemID.CelestialSigil, 1),
				new CollectCount("Sigil3", ItemID.CelestialSigil, 1),
				new CollectCount("Sigil4", ItemID.CelestialSigil, 1),
				new CollectCount("Sigil4", ItemID.CelestialSigil, 1),
			], this.GetLocalization("CollectSigils")),
		
			new InteractWithNPC("Talk", NPCQuestGiver,Language.GetText("Mods.PathOfTerraria.NPCs.AzarielNPC.Dialogue.CultistMoonlordDialogue3"),
				Language.GetText("Mods.PathOfTerraria.NPCs.AzarielNPC.Dialogue.CultistMoonlordDialogue3"),
			onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<CultistMap>())),

			// TODO: Place sigils into the altar slots. This will create a rift that the player is sucked into. Inside, the Cultist Invasion will begin on the "alternate"
			// destroyed Ravencrest.
			// This may not be the case; needs more discussion - Gabe
			
			// TODO: Once invasion is done, the cultist will spawn in ravencrest (OR youll enter their domain portal) and you must kill them.
			new ConditionCheck("Domain", _ => SubworldSystem.Current is CultistDomain, 1, this.GetLocalization("EnterDomain1")),
			new ConditionCheck("Cultist", _ => BossTracker.DownedInDomain<CultistDomain>(NPCID.CultistBoss), 1, this.GetLocalization("Boss1")),
			
			new InteractWithNPC("PostCultist", NPCQuestGiver,Language.GetText("Mods.PathOfTerraria.NPCs.AzarielNPC.Dialogue.CultistMoonlordDialogue4"),
				Language.GetText("Mods.PathOfTerraria.NPCs.AzarielNPC.Dialogue.CultistMoonlordDialogue4")),
			
			// TODO: The 4 pillars will now be spawned in your main world. You must destroy all 4 pillars, and return to Azariel.
			// (step to kill all 4 pillars)
			
			new InteractWithNPC("PostPillars", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.AzarielNPC.Dialogue.CultistMoonlordDialogue5"),
				Language.GetText("Mods.PathOfTerraria.NPCs.AzarielNPC.Dialogue.CultistMoonlordDialogue5"),
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<MoonMap>())),
			// TODO: Once again, a portal/rift will open at the altar and Azariel will follow you through into the moon lord domain.

			new ConditionCheck("MoonDomain", _ => SubworldSystem.Current is MoonLordDomain, 1, this.GetLocalization("EnterDomain2")),
			new ConditionCheck("MoonLord", _ => BossTracker.DownedInDomain<MoonLordDomain>(NPCID.MoonLordCore), 1, this.GetLocalization("Boss2")),
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		Quest empressQuest = GetLocalPlayerInstance<EoLQuest>();
		return empressQuest.Completed && NPC.downedEmpressOfLight;
	}
}

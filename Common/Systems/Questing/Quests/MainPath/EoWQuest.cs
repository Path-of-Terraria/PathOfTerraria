using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.Systems.VanillaModifications;
using PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class EoWQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<MorvenNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 7500, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new InteractWithNPC("Start", ModContent.NPCType<MorvenNPC>(), this.GetLocalization("BlacksmithInput"), this.GetLocalization("MorvenDialogue")),
			new ConditionCheck("Raven", (_) => SubworldSystem.Current is RavencrestSubworld && NPC.AnyNPCs(ModContent.NPCType<MorvenNPC>()), 1, this.GetLocalization("MorvenRaven")),
			new InteractWithNPC("MorvenAboveground", ModContent.NPCType<MorvenNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.MorvenNPC.Dialogue.Aboveground"),
			    Language.GetText("Mods.PathOfTerraria.NPCs.MorvenNPC.Dialogue.InRavencrest")),
			new ConditionCheck("Return", (_) => SubworldSystem.Current is null, 1, this.GetLocalization("ReturnToOverworld")),
			new ActionStep((_, _) =>
			{
				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					ModContent.GetInstance<BreakableOrbsHandler>().Send();
				}
				else
				{
					DisableOrbBreaking.BreakableOrbSystem.CanBreakOrb = true;
				}

				return true;
			}),
			new ConditionCheck("Orbs", (_) => DisableEvilOrbBossSpawning.ActualOrbsSmashed > 2, 1, this.GetLocalization("SmashOrbs")),
			new ConditionCheck("KillEoW", (_) => NPC.downedBoss2, 1, this.GetLocalization("KillEoW")),
			new InteractWithNPC("Finish", ModContent.NPCType<MorvenNPC>(), LocalizedText.Empty, Language.GetText("Mods.PathOfTerraria.NPCs.MorvenNPC.Dialogue.AfterBeatingEoW"))
			{
				CountsAsCompletedOnMarker = true
			},
		];
	}

	public override bool Available()
	{
		return NPC.downedBoss1;
	}

	public override string MarkerLocation()
	{
		return CurrentStep > 0 ? "Ravencrest" : "Overworld";
	}
}
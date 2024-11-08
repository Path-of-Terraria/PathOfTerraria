using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Common.Systems.VanillaModifications;
using PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
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
			new ConditionCheck((_) => ModContent.GetInstance<RavencrestSystem>().HasOverworldNPC.Contains("PathOfTerraria/MorvenNPC"), 1, this.GetLocalization("MorvenRaven")),
			new InteractWithNPC(ModContent.NPCType<MorvenNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.MorvenNPC.Dialogue.InRavencrest")),
			new ActionStep((_, _) =>
			{
				DisableOrbBreaking.BreakableOrbSystem.CanBreakOrb = true;
				return SubworldSystem.Current is null;
			}),
			new ConditionCheck((_) => DisableEvilOrbBossSpawning.ActualOrbsSmashed > 2, 1, this.GetLocalization("SmashOrbs")),
			new ConditionCheck((_) => NPC.downedBoss2, 1, this.GetLocalization("KillEoW")),
			new InteractWithNPC(ModContent.NPCType<MorvenNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.MorvenNPC.Dialogue.AfterBeatingEoW"))
			{
				CountsAsCompletedOnMarker = true
			},
		];
	}
}
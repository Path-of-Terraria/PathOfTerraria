using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Common.Systems.Runebound;
using PathOfTerraria.Content.Items.Gear.Rings.AddedDamage;
using PathOfTerraria.Content.NPCs.Town;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal sealed class TheFirstBindingQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<BlacksmithNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((player, _) =>
		{
			player.GetModPlayer<ExpModPlayer>().Exp += 12500;
			RuneboundPlayer runebound = player.GetModPlayer<RuneboundPlayer>();
			runebound.LearnedMechanic = true;
			runebound.SyncState();
		}, "12500 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new InteractWithNPC("Start", NPCQuestGiver, this.GetLocalization("TalkToThrain"), this.GetLocalization("ThrainStart"),
				onSuccess: npc =>
				{
					Player player = Main.LocalPlayer;
					RuneboundPlayer runebound = player.GetModPlayer<RuneboundPlayer>();
					runebound.RequestTutorialEncounter();
					ItemSpawner.SpawnItem<StoneRing>(npc.Center, PoTMobHelper.GetAreaLevel(), ItemRarity.Normal);
				}),
			new ConditionCheck("Defeat", player => player.GetModPlayer<RuneboundPlayer>().TutorialEncounterCompleted,
				1, this.GetLocalization("DefeatPrison"), this.GetLocalization("ThrainStart")),
			new ConditionCheck("Craft", player => player.GetModPlayer<RuneboundPlayer>().HasCraftedRunestone,
				1, this.GetLocalization("CraftRunestone"), this.GetLocalization("CraftReminder")),
			new InteractWithNPC("Finish", NPCQuestGiver, this.GetLocalization("TalkToThrain"), this.GetLocalization("ThrainFinish"))
			{
				CountsAsCompletedOnMarker = true,
			},
		];
	}

	protected override bool InternalAvailable()
	{
		if (!Main.hardMode)
		{
			return false;
		}

		Quest wallQuest = GetLocalPlayerInstance<WoFQuest>();
		return wallQuest.Completed || wallQuest.QuestNotStarted;
	}

	public override string MarkerLocation()
	{
		return "Ravencrest";
	}
}

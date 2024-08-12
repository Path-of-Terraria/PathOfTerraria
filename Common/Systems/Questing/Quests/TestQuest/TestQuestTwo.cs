using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Questing.Quests.TestQuest;

internal class TestQuestTwo : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override string Description => "This is another test quest. Simply used for testing purposes";

	public override List<QuestStep> QuestSteps =>
	[
		new CollectCount(ItemID.StoneBlock, 2),
		new KillCount(x => x.lifeMax > 100, 10, remaining => $"Kill {remaining} mobs with 100+ max life")
	];

	public override int NPCQuestGiver => -1;

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
			{
				p.GetModPlayer<ExpModPlayer>().Exp += 500;
				ItemSpawner.SpawnRandomItem(v);
				ItemSpawner.SpawnItem<FireStarter>(v);
			},
			"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];
}
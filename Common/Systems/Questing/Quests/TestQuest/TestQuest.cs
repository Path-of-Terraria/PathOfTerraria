using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Gear.Armor.Leggings;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.TestQuest;

internal class TestQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => -1;

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new CollectCount(ItemID.StoneBlock, 50),
			new ConditionCheck(p => p.ZoneSnow, 30 * 60, Language.GetText($"Mods.{PoTMod.ModName}.Quests.Exploration.Ice")),
			new KillCount(x => x.type == NPCID.Zombie || x.type == NPCID.BlueSlime, 3, QuestLocalization("Kill.SlimeZombies")),
			new KillCount(NPCID.BlueSlime, 1, QuestLocalization("Kill.Slime")),
			new ParallelQuestStep(
			[
				new KillCount(NPCID.BlueSlime, 1, QuestLocalization("Kill.Slime")),
				new KillCount(NPCID.Zombie, 1, QuestLocalization("Kill.Zombies")),
			]),
			new KillCount(x => x.lifeMax > 100, 10, QuestLocalization("Kill.HighHealth"))
		];
	}

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
			{
				p.GetModPlayer<ExpModPlayer>().Exp += 500;
				ItemSpawner.SpawnRandomItem(v);
				ItemSpawner.SpawnItem<BurningRedBoots>(v);
			},
			"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];
}
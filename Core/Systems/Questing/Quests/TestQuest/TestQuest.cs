using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Content.Items.Gear.Armor.Leggings;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Core.Systems.Questing.RewardTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Terraria;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Questing.Quests.TestQuest;
internal class TestQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	protected override List<QuestStep> _subQuests => [
		new CollectCount(ItemID.StoneBlock, 50, s => $"Collect {s} stone."),
		new ConditionCheck(p => p.ZoneSnow, "Go to the ice/snow biome", "Has gone to the ice/snow biome."),
		new KillCount(x => x.type == NPCID.Zombie || x.type == NPCID.BlueSlime, 3, (string remaining) => $"Kill {remaining} of slimes and zombies."),
		new KillCount(NPCID.BlueSlime, 1, (string remaining) => $"Kill {remaining} slime."),
		new ParallelQuestStep([
			new KillCount(NPCID.BlueSlime, 1, (string remaining) => $"Kill {remaining} slime."),
			new KillCount(NPCID.Zombie, 1, (string remaining) => $"Kill {remaining} zombie."),
		]),
		new KillCount(x => x.lifeMax > 100, 10, (string remaining) => $"Kill {remaining} mobs with 100+ max life")
	];
	public override int NPCQuestGiver => throw new NotImplementedException();
	public override List<QuestReward> QuestRewards => [
		new ActionRewards((Player p, Vector2 v) =>
		{
			p.GetModPlayer<ExpModPlayer>().Exp += 500;
			Gear.SpawnItem(v);
			Gear.SpawnGear<BurningRedBoots>(v);
		},"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];
}

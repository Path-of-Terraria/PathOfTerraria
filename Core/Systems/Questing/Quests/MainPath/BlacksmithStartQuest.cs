﻿using PathOfTerraria.Content.Items.Gear.Armor.Leggings;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Content.NPCs.Town;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Core.Systems.Questing.RewardTypes;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Questing.Quests.TestQuest;

internal class BlacksmithStartQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override string Name => "Forging a New Blade";

	protected override List<QuestStep> _subQuests =>
	[
		new ParallelQuestStep([
			new CollectCount(item => item.type == ItemID.IronOre || item.type == ItemID.LeadOre, 20, s => $"Collect {s} iron ore."),
			new CollectCount(item => item.type == ItemID.IronHammer || item.type == ItemID.LeadHammer, 1, s => $"Collect {s} iron hammer."),
		]),
		new CollectCount(ItemID.StoneBlock, 50, s => $"Collect {s} stone."),
		new CollectCount(ItemID.Wood, 20, s => $"Collect {s} wood."),
	];

	public override int NPCQuestGiver => ModContent.NPCType<Blacksmith>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
			{
				p.GetModPlayer<ExpModPlayer>().Exp += 500;
				int sword = ItemSpawner.SpawnItemFromCategory<Sword>(v);

				if (sword != -1)
				{
					Item item = Main.item[sword];
					item.GetInstanceData().Rarity = Rarity.Magic;
					PoTGlobalItem.Roll(item, Main.rand.Next(6, 11));
				}
				
				int axe = ItemSpawner.SpawnItemFromCategory<Battleaxe>(v);

				if (axe != -1)
				{
					Item item = Main.item[axe];
					item.GetInstanceData().Rarity = Rarity.Magic;
					PoTGlobalItem.Roll(item, Main.rand.Next(6, 11));
				}
			},
			"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];
}
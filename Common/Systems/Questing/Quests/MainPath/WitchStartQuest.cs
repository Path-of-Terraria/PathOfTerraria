using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;
using PathOfTerraria.Content.NPCs.Town;
using PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class WitchStartQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<MorganaNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
			{
				p.GetModPlayer<ExpModPlayer>().Exp += 500;
				//int sword = ItemSpawner.SpawnItemFromCategory<Sword>(v);

				//if (sword != -1)
				//{
				//	Item item = Main.item[sword];
				//	item.GetInstanceData().Rarity = ItemRarity.Magic;
				//	PoTItemHelper.Roll(item, Main.rand.Next(6, 11));
				//}
				
				//int axe = ItemSpawner.SpawnItemFromCategory<Battleaxe>(v);

				//if (axe != -1)
				//{
				//	Item item = Main.item[axe];
				//	item.GetInstanceData().Rarity = ItemRarity.Magic;
				//	PoTItemHelper.Roll(item, Main.rand.Next(6, 11));
				//}
			},
			"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];

	public override List<QuestStep> SetSteps()
	{
		return 
		[
			new ConditionCheck(plr =>
			{
				Dictionary<int, int> storage = GrimoirePlayer.Get(plr).GetStoredCount();

				return storage.TryGetValue(ModContent.ItemType<BatWings>(), out int wing) && storage.TryGetValue(ModContent.ItemType<OwlFeather>(), out int feather)
					&& wing > 0 && feather > 1;
			}, 1, Language.GetText($"Mods.{PoTMod.ModName}.NPCs.MorganaNPC.QuestCondition")),
			new InteractWithNPC(ModContent.NPCType<MorganaNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.Quest2")),
			new ConditionCheck(p => p.GetModPlayer<GrimoirePlayer>().CurrentSummonId > -1, 1, Language.GetText($"Mods.{PoTMod.ModName}.NPCs.MorganaNPC.SummonCondition")),
			new InteractWithNPC(ModContent.NPCType<MorganaNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.MorganaNPC.Dialogue.Quest3")),
		];
	}

	public override string MarkerLocation()
	{
		return "Ravencrest";
	}
}
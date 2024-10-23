using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;
using PathOfTerraria.Content.NPCs.Town;
using PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;
using PathOfTerraria.Core.Items;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class WitchStartQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<WitchNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
			{
				p.GetModPlayer<ExpModPlayer>().Exp += 500;
				int sword = ItemSpawner.SpawnItemFromCategory<Sword>(v);

				if (sword != -1)
				{
					Item item = Main.item[sword];
					item.GetInstanceData().Rarity = ItemRarity.Magic;
					PoTItemHelper.Roll(item, Main.rand.Next(6, 11));
				}
				
				int axe = ItemSpawner.SpawnItemFromCategory<Battleaxe>(v);

				if (axe != -1)
				{
					Item item = Main.item[axe];
					item.GetInstanceData().Rarity = ItemRarity.Magic;
					PoTItemHelper.Roll(item, Main.rand.Next(6, 11));
				}
			},
			"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];

	public override List<QuestStep> SetSteps()
	{
		return 
		[
			new ConditionCheck(plr =>
			{
				Dictionary<int, int> storage = plr.GetModPlayer<GrimoireStoragePlayer>().GetStoredCount();

				return storage.TryGetValue(ModContent.ItemType<BatWings>(), out int wing) && storage.TryGetValue(ModContent.ItemType<OwlFeather>(), out int feather)
					&& wing > 0 && feather > 1;
			}, 1, Language.GetText($"Mods.{PoTMod.ModName}.NPCs.WitchNPC.QuestCondition")),
			new InteractWithNPC(ModContent.NPCType<WitchNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.WitchNPC.Dialogue.Quest2")),
			new ConditionCheck(p => p.ownedProjectileCounts[ModContent.ProjectileType<OwlSummon>()] > 0, 1, Language.GetText($"Mods.{PoTMod.ModName}.NPCs.WitchNPC.SummonCondition")),
			new InteractWithNPC(ModContent.NPCType<WitchNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.WitchNPC.Dialogue.Quest3")),
		];
	}
}
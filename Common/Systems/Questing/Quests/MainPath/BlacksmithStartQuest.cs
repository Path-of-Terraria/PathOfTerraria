﻿using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Content.NPCs.Town;
using PathOfTerraria.Content.Skills.Melee;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class BlacksmithStartQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<BlacksmithNPC>();

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
			new ParallelQuestStep([
				new CollectCount(item => item.type == ItemID.IronOre || item.type == ItemID.LeadOre, 20, Lang.GetItemName(ItemID.IronOre)),
				new CollectCount(item => item.type == ItemID.IronHammer || item.type == ItemID.LeadHammer, 1, Lang.GetItemName(ItemID.IronHammer)),
			]),
			new CollectCount(ItemID.StoneBlock, 50),
			new CollectCount(ItemID.Wood, 20),
			new ActionStep((_, _) => 
			{
				RavencrestSystem.UpgradeBuilding("Forge");
				return true;
			}),
			new InteractWithNPC(ModContent.NPCType<BlacksmithNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.Quest2")),
			new ActionStep((_, _) => {
				int npc = NPC.FindFirstNPC(ModContent.NPCType<BlacksmithNPC>());
				Item.NewItem(new EntitySource_Gift(Main.npc[npc]), Main.npc[npc].Center, ModContent.ItemType<IronBroadsword>());
				return true;
			}),
			new ParallelQuestStep([
				new CollectCount(ItemID.StoneBlock, 50),
				new CollectCount(ItemID.Wood, 50),
				new KillCount(NPCID.Zombie, 15, Localize("Kill.Zombies")),
			]),
			new ActionStep((_, _) =>
			{
				RavencrestSystem.UpgradeBuilding("Forge");
				return true;
			}),
			new InteractWithNPC(ModContent.NPCType<BlacksmithNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.Quest3")),
			new ActionStep((_, _) =>
			{
				Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().TryAddSkill(new Berserk());
				return true;
			})
		];
	}
}
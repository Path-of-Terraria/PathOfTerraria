using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Content.NPCs.Town;
using PathOfTerraria.Content.Skills.Melee;
using PathOfTerraria.Content.Skills.Ranged;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class HunterStartQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<HunterNPC>();

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
			new ActionStep((_, _) => true),
			new InteractWithNPC(ModContent.NPCType<HunterNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.HunterNPC.Dialogue.Quest2"),
			[
				(ItemID.Silk, 20), (ItemID.Wood, 50), (ItemID.StoneBlock, 50),
			], true),
			new ActionStep((_, _) =>
			{
				RavencrestSystem.UpgradeBuilding("Lodge", 1);

				int npc = NPC.FindFirstNPC(ModContent.NPCType<HunterNPC>());
				Item.NewItem(new EntitySource_Gift(Main.npc[npc]), Main.npc[npc].Center, ModContent.ItemType<WoodenBow>());
				return true;
			}),
			new KillCount(npc => npc.type is NPCID.DemonEye or NPCID.Crimera or NPCID.EaterofSouls, 10, Localize("Kill.FloatingMisc")),
			new InteractWithNPC(ModContent.NPCType<HunterNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.HunterNPC.Dialogue.Quest3"),
			[
				(ItemID.Wood, 40), (ItemID.Gel, 10), (ItemID.StoneBlock, 20)
			], true),
			new ActionStep((_, _) =>
			{
				RavencrestSystem.UpgradeBuilding("Lodge", 2);
				Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().TryAddSkill(new RainOfArrows());
				return true;
			})
		];
	}
}
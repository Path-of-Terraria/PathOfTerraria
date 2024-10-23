using System.Collections.Generic;
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
			new InteractWithNPC(ModContent.NPCType<BlacksmithNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.Quest2"),
			[
				new GiveItem(20, ItemID.IronOre, ItemID.LeadOre), new(1, ItemID.IronHammer, ItemID.LeadHammer), new(50, ItemID.StoneBlock), new(20, ItemID.Wood)
			], true),
			new ActionStep((_, _) => 
			{
				RavencrestSystem.UpgradeBuilding("Forge");

				int npc = NPC.FindFirstNPC(ModContent.NPCType<BlacksmithNPC>());
				Item.NewItem(new EntitySource_Gift(Main.npc[npc]), Main.npc[npc].Center, ModContent.ItemType<IronBroadsword>());
				return true;
			}),
			new KillCount(NPCID.Zombie, 15, this.GetLocalization("Kill.Zombies")),
			new InteractWithNPC(ModContent.NPCType<BlacksmithNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.Quest3"),
			[
				new GiveItem(30, ItemID.StoneBlock), new(50, ItemID.Wood), new(10, ItemID.GoldBar, ItemID.PlatinumBar)
			], true) { CountsAsCompletedOnMarker = true },
			new ActionStep((_, _) =>
			{
				RavencrestSystem.UpgradeBuilding("Forge");
				Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().TryAddSkill(new Berserk());
				return true;
			}) { CountsAsCompletedOnMarker = true }
		];
	}
}
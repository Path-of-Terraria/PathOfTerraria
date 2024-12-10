using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Content.Items.Gear.Weapons.Wand;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class WoFQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<WizardNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 5000, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("WizardStart"),
				null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<TinyHat>())),
			new ParallelQuestStep([
				new InteractWithNPC(ModContent.NPCType<BlacksmithNPC>(), this.GetLocalization("ThrainHelp"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<DwarvenGreatsword>())),
				new InteractWithNPC(ModContent.NPCType<HunterNPC>(), this.GetLocalization("ElaraHelp"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<Twinbow>())),
				new InteractWithNPC(ModContent.NPCType<WitchNPC>(), this.GetLocalization("MorganaHelp"),
					null, false, (npc) => 
					{
						for (int i = 0; i < 3; ++i)
						{
							Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<SoulfulAsh>());
							Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<FlamingEye>());
						}
					}),
			]),
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("WizardContinue"),
				null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<VoidPearl>())),
			new ConditionCheck(_ => Main.hardMode, 1, this.GetLocalization("KillWall")),
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("WizardFinish")),
		];
	}
}
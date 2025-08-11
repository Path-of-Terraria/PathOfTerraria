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
using Terraria.Localization;

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
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("WizardStart"), LocalizedText.Empty,
				null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<TinyHat>())),
			new ParallelQuestStep([
				new InteractWithNPC(ModContent.NPCType<BlacksmithNPC>(), this.GetLocalization("ThrainHelp"), this.GetLocalization("WizardStart"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<DwarvenGreatsword>())),
				new InteractWithNPC(ModContent.NPCType<HunterNPC>(), this.GetLocalization("ElaraHelp"), this.GetLocalization("WizardStart"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<Twinbow>())),
				new InteractWithNPC(ModContent.NPCType<MorganaNPC>(), this.GetLocalization("MorganaHelp"), this.GetLocalization("WizardStart"),
					null, false, (npc) => 
					{
						for (int i = 0; i < 3; ++i)
						{
							int item = Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<SoulfulAsh>());

							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
							}

							item = Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<FlamingEye>());

							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
							}
						}
					}),
			]),
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("WizardContinue"), LocalizedText.Empty,
				null, false, (npc) =>
				{
					int item = Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<VoidPearl>());

					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
					}
				}),
			new ConditionCheck(_ => Main.hardMode, 1, this.GetLocalization("KillWall")),
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("WizardFinish")),
		];
	}

	public override bool Available()
	{
		return NPC.downedDeerclops.ToInt() + NPC.downedQueenBee.ToInt() + NPC.downedBoss3.ToInt() + NPC.downedBoss2.ToInt() >= 3;
	}

	public override string MarkerLocation()
	{
		return "Ravencrest";
	}
}
using PathOfTerraria.Common.Quests;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Content.Items.Gear.Weapons.Wand;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using System.Collections.Generic;
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
			new InteractWithNPC("Start", NPCQuestGiver, this.GetLocalization("WizardStart"), LocalizedText.Empty,
				null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<TinyHat>())),
			new ParallelQuestStep("Branch", [
				new InteractWithNPC("Thrain", ModContent.NPCType<BlacksmithNPC>(), this.GetLocalization("WizardStart"), this.GetLocalization("ThrainHelp"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<DwarvenGreatsword>())),
				new InteractWithNPC("Elara", ModContent.NPCType<HunterNPC>(), this.GetLocalization("WizardStart"), this.GetLocalization("ElaraHelp"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<Twinbow>())),
				new InteractWithNPC("Morgana", ModContent.NPCType<MorganaNPC>(), this.GetLocalization("WizardStart"), this.GetLocalization("MorganaHelp"),
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
			], this.GetLocalization("WizardStart")),
			new InteractWithNPC("Continue", NPCQuestGiver, LocalizedText.Empty, this.GetLocalization("WizardContinue"),
				null, false, (npc) =>
				{
					int item = Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<VoidPearl>());

					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
					}
				}),
			new ConditionCheck("Kill", _ => BossTracker.DownedInDomain<WallOfFleshDomain>(NPCID.WallofFlesh), 1, this.GetLocalization("KillWall"))
			{
				SkipCheck = QuestUtils.BossSkipCheck(NPCID.WallofFlesh)
			},
			new InteractWithNPC("Finish", NPCQuestGiver, this.GetLocalization("WizardFinish"), 
				onSuccess: _ => Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<QueenSlimeQuest>()),
		];
	}

	public override bool Available()
	{
		return NPC.downedDeerclops.ToInt() + NPC.downedQueenBee.ToInt() + NPC.downedBoss3.ToInt() >= 2 && NPC.downedBoss2;
	}

	public override string MarkerLocation()
	{
		return "Ravencrest";
	}
}
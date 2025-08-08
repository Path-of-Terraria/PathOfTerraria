using PathOfTerraria.Common.NPCs.ConditionalDropping;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class EoCQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<EldricNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 5000, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new ActionStep((_, _) =>
			{
				Main.LocalPlayer.GetModPlayer<ConditionalDropPlayer>().AddId<LunarShard>();
				return true;
			}),
			new ParallelQuestStep(
			[
				new InteractWithNPC(ModContent.NPCType<GarrickNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.EoCQuestLine"),
					null, false, (npc) => 
					{
						int item = Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<LunarLiquid>());

						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
						}
					}),
				new InteractWithNPC(ModContent.NPCType<EldricNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.EldricNPC.Dialogue.QuestLunar"),
					[
						new GiveItem(5, ModContent.ItemType<LunarShard>())
					], true),
			]),
			new ActionStep((_, _) =>
			{
				RavencrestSystem.UpgradeBuilding("Observatory");
				return true;
			}),
			new InteractWithNPC(ModContent.NPCType<EldricNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.EldricNPC.Dialogue.Quest2"),
				[ new GiveItem(1, ModContent.ItemType<LunarLiquid>()) ], true, (npc) =>
				{
					int item = Item.NewItem(new EntitySource_Gift(npc), npc.Bottom, ModContent.ItemType<LunarObject>());
					Main.item[item].shimmered = true; // So it doesn't immediately shatter + cool effect

					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
					}
				}),
			new KillCount(NPCID.EyeofCthulhu, 1, this.GetLocalization("Kill.EoC")),
			new ActionStep((_, _) =>
			{
				RavencrestSystem.UpgradeBuilding("Observatory");
				return true;
			}),
			new InteractWithNPC(ModContent.NPCType<EldricNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.EldricNPC.Dialogue.Quest3"))
			{
				CountsAsCompletedOnMarker = true
			},
			new ActionStep((_, _) => {
				RavencrestSystem.UpgradeBuilding("Observatory");
				Main.LocalPlayer.GetModPlayer<ConditionalDropPlayer>().RemoveId<LunarShard>();
				return true;
			}) { CountsAsCompletedOnMarker = true },
		];
	}

	public override bool Available()
	{
		return BossTracker.TotalBossesDowned.Contains(NPCID.KingSlime);
	}

	public override string MarkerLocation()
	{
		return "Ravencrest";
	}
}
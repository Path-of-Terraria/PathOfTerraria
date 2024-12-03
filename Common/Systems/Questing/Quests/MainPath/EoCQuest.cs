using System.Collections.Generic;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

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
				ConditionalDropHandler.AddId<LunarShard>();
				return true;
			}),
			new ParallelQuestStep(
			[
				new InteractWithNPC(ModContent.NPCType<GarrickNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.GarrickNPC.Dialogue.EoCQuestLine"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<LunarLiquid>())),
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
				ConditionalDropHandler.RemoveId<LunarShard>();
				return true;
			}) { CountsAsCompletedOnMarker = true },
		];
	}
}
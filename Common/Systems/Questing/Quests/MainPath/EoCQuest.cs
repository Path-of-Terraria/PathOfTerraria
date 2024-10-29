using System.Collections.Generic;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
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
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 500,
			"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new ActionStep((_, _) => 
			{
				SyncedCustomDrops.AddId<LunarShard>();
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
				SyncedCustomDrops.RemoveId<LunarShard>();
				return true;
			}),
			new InteractWithNPC(ModContent.NPCType<EldricNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.EldricNPC.Dialogue.Quest2"), 
				null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<LunarObject>()))
		];
	}
}
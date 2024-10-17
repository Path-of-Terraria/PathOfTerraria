using System.Collections.Generic;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Gear.Weapons.Staff;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using PathOfTerraria.Content.Skills.Magic;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class WizardStartQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<WizardNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => 
		{
			p.GetModPlayer<SkillCombatPlayer>().TryAddSkill(new Nova());
			ItemSpawner.SpawnItemFromCategory<Staff>(v, 0, Enums.ItemRarity.Magic);
		}, "Obtain the Nova buff and a Magic rarity Staff."),
	];

	public override List<QuestStep> SetSteps()
	{
		return 
		[
			new ActionStep((player, _) => 
			{
				ConditionalDropHandler.AddId(ModContent.ItemType<TomeOfTheElders>());
				return true;
			}),
			new InteractWithNPC(ModContent.NPCType<WizardNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.WizardNPC.Dialogue.Quest2"),
			[
				new GiveItem(1, ModContent.ItemType<TomeOfTheElders>())
			], true),
			new ActionStep((player, _) =>
			{
				ConditionalDropHandler.RemoveId(ModContent.ItemType<TomeOfTheElders>());
				return true;
			}) { CountsAsCompletedOnMarker = true },
		];
	}
}
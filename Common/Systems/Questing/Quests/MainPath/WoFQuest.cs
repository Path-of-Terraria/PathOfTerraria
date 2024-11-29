using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
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
				null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<LunarLiquid>())),
			new ParallelQuestStep([
				new InteractWithNPC(ModContent.NPCType<BlacksmithNPC>(), this.GetLocalization("ThrainHelp"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<LunarLiquid>())),
				new InteractWithNPC(ModContent.NPCType<HunterNPC>(), this.GetLocalization("ElaraHelp"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<LunarLiquid>())),
				new InteractWithNPC(ModContent.NPCType<WitchNPC>(), this.GetLocalization("MorganaHelp"),
					null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<LunarLiquid>())),
			]),
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("WizardContinue"),
				null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<LunarLiquid>())),
			new KillCount(NPCID.WallofFlesh, 1, this.GetLocalization("KillWall")),
			new InteractWithNPC(NPCQuestGiver, this.GetLocalization("WizardFinish"),
				null, false, (npc) => Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, ModContent.ItemType<LunarLiquid>())),
		];
	}
}
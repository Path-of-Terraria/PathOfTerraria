using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Content.Items.Placeable;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class FirstQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => -1;

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 500, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new ConditionCheck(plr => plr.DistanceSQ(ModContent.GetInstance<RavencrestSystem>().EntrancePosition.ToWorldCoordinates()) < 400 * 400,
				1, this.GetLocalization("ApproachEntrance")),
			new ConditionCheck(_ => SubworldSystem.Current is RavencrestSubworld, 1, this.GetLocalization("EnterRavencrest")),
			new ActionStep((plr, step) =>
			{
				int item = Item.NewItem(new EntitySource_Misc("Quest"), plr.Bottom, ModContent.ItemType<ArcaneObeliskItem>());

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
				}

				return true;
			})
		];
	}

	public override bool Available()
	{
		return true;
	}

	public override string MarkerLocation()
	{
		return "";
	}
}
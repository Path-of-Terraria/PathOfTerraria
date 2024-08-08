using PathOfTerraria.Common.Events;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

// maby we make a dialouge class that is for questing dialouge?
// *so that we can talk with the npc*
internal class ConditionCheck(Func<Player, bool> condition, string displayText, string completeText) : QuestStep
{
	public override string QuestString()
	{
		return displayText;
	}

	public override string QuestCompleteString()
	{
		return completeText;
	}

	public override bool Track(Player player)
	{
		return condition(player);
	}
}
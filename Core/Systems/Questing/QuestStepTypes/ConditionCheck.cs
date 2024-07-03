using PathOfTerraria.Core.Events;

namespace PathOfTerraria.Core.Systems.Questing.QuestStepTypes;

// maby we make a dialouge class that is for questing dialouge?
// *so that we can talk with the npc*
internal class ConditionCheck(Func<Player, bool> condition, string displayText, string completeText) : QuestStep
{
	private PathOfTerrariaPlayerEvents.PostUpdateDelegate tracker;

	public override string QuestString()
	{
		return displayText;
	}

	public override string QuestCompleteString()
	{
		return completeText;
	}

	public override void Track(Player player, Action onCompletion)
	{
		tracker = (Player p) =>
		{
			if (condition(p))
			{
				onCompletion();
			}
		};

		PathOfTerrariaPlayerEvents.PostUpdateEvent += tracker;
	}

	public override void UnTrack()
	{
		PathOfTerrariaPlayerEvents.PostUpdateEvent -= tracker;
	}
}
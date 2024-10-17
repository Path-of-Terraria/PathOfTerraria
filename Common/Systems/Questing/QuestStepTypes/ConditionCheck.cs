using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

/// <summary>
/// Step that asks for a condition, and optionally, a certain amount of time under that condition.<br/>
/// For example, <paramref name="condition"/> of <c>player => player.ZoneForest</c> and an <paramref name="exploreTime"/> of <c>30 * 60</c> 
/// would complete after the user is in the Forest for 30 seconds.<br/>
/// <paramref name="displayText"/> should take 1 format parameters for exploration time left/max time.
/// </summary>
/// <param name="condition"></param>
/// <param name="displayText"></param>
internal class ConditionCheck(Func<Player, bool> condition, float exploreTime, LocalizedText displayText) : QuestStep
{
	private readonly float ExploreTime = exploreTime;

	private float _explore = 0;

	public override string DisplayString()
	{
		return displayText.WithFormatArgs(IsDone ? 100 : (_explore / ExploreTime * 100).ToString("#0.##")).Value;
	}

	public override bool Track(Player player)
	{
		if (condition(player))
		{
			_explore++;
		}

		return _explore > ExploreTime;
	}
}
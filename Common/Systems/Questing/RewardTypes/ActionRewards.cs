namespace PathOfTerraria.Common.Systems.Questing.RewardTypes;

internal class ActionRewards(Action<Player, Vector2> rewards, string rewardString) : QuestReward
{
	public override string RewardString()
	{
		return rewardString;
	}

	public override void GiveReward(Player player, Vector2 dropPosition)
	{
		rewards.Invoke(player, dropPosition);
	}
}
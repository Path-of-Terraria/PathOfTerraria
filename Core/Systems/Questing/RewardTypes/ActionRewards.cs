using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Core.Systems.Questing.RewardTypes;
internal class ActionRewards(Action<Player, Vector2> rewards, string rewardString) : QuestReward
{
	private readonly Action<Player, Vector2> _rewards = rewards;
	public override string RewardString()
	{
		return rewardString;
	}
	public override void GiveReward(Player player, Vector2 dropPosition)
	{
		_rewards.Invoke(player, dropPosition);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfTerraria.Core.Systems.Questing.RewardTypes;
internal class ItemsReward(List<int> rewards) : QuestReward
{
	private readonly List<int> _rewards = rewards;
	public override string RewardString()
	{
		return base.RewardString();
	}
	public override void GiveReward(Player player, Vector2 dropPosition)
	{
		// drop rewards...
	}
}

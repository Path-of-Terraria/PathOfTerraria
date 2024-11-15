using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Common.Systems.Questing.RewardTypes;

internal class SkillPointsReward(int amount) : QuestReward
{
	public override void GiveReward(Player player, Vector2 dropPosition)
	{
		player.GetModPlayer<PassiveTreePlayer>().Points++;
		player.GetModPlayer<ExpModPlayer>().QuestLevel++;
	}

	public override string RewardString()
	{
		return $"+{amount} passive tree skill points";
	}
}
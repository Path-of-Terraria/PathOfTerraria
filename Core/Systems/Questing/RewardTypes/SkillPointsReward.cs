using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Core.Systems.Questing.RewardTypes;

internal class SkillPointsReward(int amount) : QuestReward
{
	public override void GiveReward(Player player, Vector2 dropPosition)
	{
		player.GetModPlayer<TreePlayer>().Points++;
		player.GetModPlayer<ExpModPlayer>().QuestLevel++;
	}

	public override string RewardString()
	{
		return $"+{amount} passive tree skill points";
	}
}
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.TreeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.IO;

namespace PathOfTerraria.Core.Systems.Questing.RewardTypes;
internal class SkillPointsReward(int amount) : QuestReward
{
	private readonly int _amount = amount;

	public override void GiveReward(Player player, Vector2 dropPosition)
	{
		player.GetModPlayer<TreePlayer>().Points++;
		player.GetModPlayer<ExpModPlayer>().QuestLevel++;
	}

	public override string RewardString()
	{
		return $"+{_amount} passiv tree skill points";
	}
}
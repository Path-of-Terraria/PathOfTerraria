using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

// -{Value}% Mana Costs
internal class DecreasedManaCostPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.manaCost *= 1 - (Value / 100.0f) * Level;
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.CostModifier *= 0.9f;
	}
}

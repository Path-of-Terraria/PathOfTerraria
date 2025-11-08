using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

// -{Value}% Mana Costs
internal class DecreasedManaCostPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.manaCost *= 1 - (Value / 100.0f) * Level;
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.ManaCost *= 0.9f;
	}
}

using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc;

internal class ReducedSkillCooldownPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.Cooldown *= 1 - Value / 100f;
	}
}

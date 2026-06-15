using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;

namespace PathOfTerraria.Content.Passives;

internal class BiggerExplosivesPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float areaBonus = Value / 100f;
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ExplosionSize += areaBonus;
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.AreaOfEffect += areaBonus;
	}
}
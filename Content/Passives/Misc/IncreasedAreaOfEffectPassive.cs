using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedAreaOfEffectPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float areaBonus = (Value / 100f) * Level;
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ExplosionSize += areaBonus;
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.AreaOfEffect += areaBonus;
	}
}

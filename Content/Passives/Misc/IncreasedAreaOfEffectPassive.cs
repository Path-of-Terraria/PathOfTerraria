using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc;

internal class IncreasedAreaOfEffectPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float bonus = Value / 100f;
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.AreaOfEffect += bonus;
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.AreaOfEffect += bonus;
	}
}

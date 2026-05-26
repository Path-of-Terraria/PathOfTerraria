using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc.Masteries;

internal class TemporalCompressionMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		float modifier = Value / 100f;
		// Buffs
		player.GetModPlayer<BuffModifierPlayer>().BuffBonus += modifier;
		// Skills
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.Duration += modifier;
		// Projectiles
		player.GetModPlayer<DurationPlayer>().DurationModifier += modifier;
	}
}
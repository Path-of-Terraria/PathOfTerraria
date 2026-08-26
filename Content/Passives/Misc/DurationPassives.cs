using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc;

internal class DebuffsExpireFasterPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float modifier = Math.Max(0f, 1f - (Value / 100f));
		player.GetModPlayer<BuffModifierPlayer>().ResistanceStrength *= modifier;
	}
}

internal class IncreasedDurationPassive : Passive
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

internal class DecreasedDurationPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float modifier = Value / 100f;
		// Buffs
		player.GetModPlayer<BuffModifierPlayer>().BuffBonus -= modifier;
		// Skills
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.Duration -= modifier;
		// Projectiles
		player.GetModPlayer<DurationPlayer>().DurationModifier -= modifier;
	}
}


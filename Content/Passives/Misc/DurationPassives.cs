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

/// <summary>
/// Increases the duration of all buffs, skills, and player-spawned projectiles by
/// <see cref="Passive.Value"/> percent per allocated level.
/// </summary>
internal class IncreasedDurationPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float modifier = Value / 100f * Level;
		player.GetModPlayer<BuffModifierPlayer>().BuffBonus += modifier;
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.Duration += modifier;
		player.GetModPlayer<DurationPlayer>().IncreasedDuration += modifier;
	}
}
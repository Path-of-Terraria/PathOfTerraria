using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// Tracks the global increased duration multiplier accumulated from passives and other
/// sources.  A value of <c>0.25f</c> means +25% increased duration.
/// </summary>
/// <remarks>
/// This player is read by <see cref="DurationGlobalProjectile"/> to scale
/// <see cref="Projectile.timeLeft"/> for all player-spawned projectiles, and is written
/// to by passives such as <c>IncreasedDurationPassive</c>.  Buff and skill durations are
/// handled directly via <see cref="BuffModifierPlayer.BuffBonus"/> and
/// <see cref="SkillPlayers.SkillCombatPlayer.GlobalBuff"/> respectively.
/// </remarks>
public class DurationPlayer : ModPlayer
{
	/// <summary>
	/// Accumulated bonus to all durations this frame, as a fraction (e.g. 0.25 = +25%).
	/// Reset to zero each frame by <see cref="ResetEffects"/>.
	/// </summary>
	public float IncreasedDuration;

	public override void ResetEffects()
	{
		IncreasedDuration = 0f;
	}
}

/// <summary>
/// Scales <see cref="Projectile.timeLeft"/> on spawn for projectiles owned by a player
/// who has a non-zero <see cref="DurationPlayer.IncreasedDuration"/>.
/// </summary>
internal class DurationGlobalProjectile : GlobalProjectile
{
	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		Player player = source switch
		{
			EntitySource_Parent { Entity: Player p } => p,
			EntitySource_ItemUse_WithAmmo { Player: Player p } => p,
			EntitySource_ItemUse { Player: Player p } => p,
			_ => null
		};

		if (player is null)
		{
			return;
		}

		float bonus = player.GetModPlayer<DurationPlayer>().IncreasedDuration;

		if (bonus == 0f)
		{
			return;
		}

		projectile.timeLeft = (int)(projectile.timeLeft * (1f + bonus));
	}
}

using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Common;

public static class EntityExtensions
{
	/// <summary>
	/// Safely normalizes a vector directed at <paramref name="target"/>.<br/>
	/// Default fallback value is <see cref="Vector2.Zero"/>.
	/// </summary>
	/// <param name="entity">Entity to reference.</param>
	/// <param name="target">Target to point to.</param>
	/// <param name="defaultValue">Default value if the normalization wasn't valid (had NaNs). Defaults to <see cref="Vector2.Zero"/>.</param>
	/// <returns>The unit vector pointing towards <paramref name="target"/>, or <paramref name="defaultValue"/>.</returns>
	public static Vector2 SafeDirectionTo(this Entity entity, Vector2 target, Vector2? defaultValue = null)
	{
		return (target - entity.Center).SafeNormalize(defaultValue ?? Vector2.Zero);
	}

	/// <summary>
	/// Safely normalizes a vector directed away from <paramref name="target"/>.<br/>
	/// Default fallback value is <see cref="Vector2.Zero"/>.
	/// </summary>
	/// <param name="entity">Entity to reference.</param>
	/// <param name="target">Target to point away from.</param>
	/// <param name="defaultValue">Default value if the normalization wasn't valid (had NaNs). Defaults to <see cref="Vector2.Zero"/>.</param>
	/// <returns>The unit vector pointing away from <paramref name="target"/>, or <paramref name="defaultValue"/>.</returns>
	public static Vector2 SafeDirectionFrom(this Entity entity, Vector2 target, Vector2? defaultValue = null)
	{
		return (entity.Center - target).SafeNormalize(defaultValue ?? Vector2.Zero);
	}

	internal static SkillSpecial GetSkillSpecialization<TSkill>(this Player player) where TSkill : Skill
	{
		return player.GetModPlayer<SkillTreePlayer>().GetSpecialization<TSkill>();
	}

	internal static bool HasSkillSpecialization<TSkill, TSpecialization>(this Player player) where TSkill : Skill where TSpecialization : SkillSpecial
	{
		return player.GetModPlayer<SkillTreePlayer>().HasSpecialization<TSkill, TSpecialization>();
	}

	/// <inheritdoc cref="SkillTreePlayer.GetPassiveStrength{TTree, TPassive}"/>
	/// <param name="player">The player being referenced.</param>
	internal static int GetPassiveStrength<TTree, TPassive>(this Player player) where TTree : SkillTree where TPassive : SkillPassive
	{
		return player.GetModPlayer<SkillTreePlayer>().GetPassiveStrength<TTree, TPassive>();
	}

	/// <inheritdoc cref="SkillTreePlayer.HasPassive{TTree, TPassive}"/>
	/// <param name="player">The player being referenced.</param>
	internal static bool HasTreePassive<TTree, TPassive>(this Player player) where TTree : SkillTree where TPassive : SkillPassive
	{
		return player.GetModPlayer<SkillTreePlayer>().HasPassive<TTree, TPassive>();
	}

	/// <summary>
	/// Gets the owner for a projectile. Assumes this is a player-owned projectile; DO NOT USE for server owned projectiles, or projectiles that may be server owned!
	/// </summary>
	internal static Player GetOwner(this Projectile projectile)
	{
		return Main.player[projectile.owner];
	}
}

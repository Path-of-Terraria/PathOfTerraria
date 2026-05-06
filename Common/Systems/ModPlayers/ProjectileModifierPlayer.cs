using System.Collections.Generic;
using PathOfTerraria.Common.Projectiles;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class ProjectileModifierProjectile : GlobalProjectile
{
	/// <summary>
	/// Tags a projectile spawned directly by a player (such as where <c>source is EntitySource_Parent { Entity: Player }</c>) as modifiable even if
	/// it's not a <see cref="EntitySource_ItemUse_WithAmmo"/>.
	/// </summary>
	internal const string ModifiableProjectileTag = "Modifiable";

	private static readonly HashSet<int> InvalidProjectilesToMove = [];

	public override void SetStaticDefaults()
	{
		InvalidProjectilesToMove.Clear();
		InvalidProjectilesToMove.Add(ProjectileID.FinalFractal);
	}

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		Player player = null;
		Item item = null;
		bool valid = false;

		if (source is EntitySource_ItemUse_WithAmmo { Player: Player plr, Item: Item sourceItem })
		{
			valid = true;
			player = plr;
			item = sourceItem;
		}

		if (source is EntitySource_Parent { Entity: Player plr2 } && source.Context is { } context && context.Contains(ModifiableProjectileTag))
		{
			valid = true;
			player = plr2;
		}

		if (valid)
		{
			EntityModifier mods = player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier;
			StatModifier speed = mods.ProjectileSpeed;
			StatModifier damage = mods.ProjectileDamage;

			if (speed != StatModifier.Default && !InvalidProjectilesToMove.Contains(projectile.type))
			{
				projectile.velocity *= speed.ApplyTo(1f);
				int extras = (int)(projectile.velocity.Length() / 16f);

				if (extras > 0)
				{
					projectile.extraUpdates += extras;
					projectile.velocity /= extras;
				}
			}

			if (damage != StatModifier.Default)
			{
				projectile.damage = (int)damage.ApplyTo(projectile.damage);
			}

			TryStartContinuousDamageUpdates(projectile, player, item);
		}
	}

	private void TryStartContinuousDamageUpdates(Projectile projectile, Player player, Item item)
	{
		if (player is null || item is null || projectile.damage <= 0 || !ShouldContinuouslyUpdateDamage(projectile, item))
		{
			return;
		}

		StatModifier damageModifier = player.GetTotalDamage(projectile.DamageType);
		projectile.originalDamage = Math.Max(1, (int)Unapply(damageModifier, projectile.damage));
		projectile.ContinuouslyUpdateDamageStats = true;
	}

	private static bool ShouldContinuouslyUpdateDamage(Projectile projectile, Item item)
	{
		return projectile.ModProjectile is not IOnContinuouslyUpdateDamage
			&& !projectile.minion
			&& !projectile.sentry
			&& (projectile.aiStyle == ProjAIStyleID.Yoyo || item.channel);
	}

	private static float Unapply(StatModifier modifier, float damage)
	{
		float divisor = modifier.Additive * modifier.Multiplicative;

		if (MathF.Abs(divisor) < 0.0001f)
		{
			return damage;
		}

		return (damage - modifier.Flat) / divisor - modifier.Base;
	}
}

using System.Collections.Generic;
using PathOfTerraria.Common.Projectiles;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class ProjectileModifierProjectile : GlobalProjectile, IOnContinuouslyUpdateDamage
{
	/// <summary>
	/// Tags a projectile spawned directly by a player (such as where <c>source is EntitySource_Parent { Entity: Player }</c>) as modifiable even if
	/// it's not a <see cref="EntitySource_ItemUse_WithAmmo"/>.
	/// </summary>
	internal const string ModifiableProjectileTag = "Modifiable";

	private static readonly HashSet<int> InvalidProjectilesToMove = [];

	private bool continuouslyUpdateDamage;
	private float unmodifiedDamage;
	private int lastUpdatedDamage;
	private int owner = -1;
	private int projectileIndex = -1;

	public override bool InstancePerEntity => true;

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

	public void OnContinuouslyUpdateDamage()
	{
		UpdateDamage(Main.projectile.IndexInRange(projectileIndex) ? Main.projectile[projectileIndex] : null);
	}

	public override void PostAI(Projectile projectile)
	{
		UpdateDamage(projectile);
	}

	private void UpdateDamage(Projectile projectile)
	{
		if (!continuouslyUpdateDamage || projectile is null || !Main.player.IndexInRange(owner))
		{
			return;
		}

		Player player = Main.player[owner];

		if (!player.active || !projectile.active)
		{
			return;
		}

		int damage = Math.Max(1, (int)player.GetTotalDamage(projectile.DamageType).ApplyTo(unmodifiedDamage));

		if (damage == lastUpdatedDamage)
		{
			return;
		}

		projectile.damage = damage;
		projectile.originalDamage = damage;
		projectile.netUpdate = true;
		lastUpdatedDamage = damage;
	}

	private void TryStartContinuousDamageUpdates(Projectile projectile, Player player, Item item)
	{
		if (player is null || item is null || projectile.damage <= 0 || !ShouldContinuouslyUpdateDamage(projectile, item))
		{
			return;
		}

		StatModifier damageModifier = player.GetTotalDamage(projectile.DamageType);
		unmodifiedDamage = Unapply(damageModifier, projectile.damage);
		lastUpdatedDamage = projectile.damage;
		owner = player.whoAmI;
		projectileIndex = projectile.whoAmI;
		continuouslyUpdateDamage = unmodifiedDamage > 0;
	}

	private static bool ShouldContinuouslyUpdateDamage(Projectile projectile, Item item)
	{
		return projectile.ModProjectile is not IOnContinuouslyUpdateDamage
			&& (projectile.aiStyle == ProjAIStyleID.Yoyo || projectile.minion || projectile.sentry || item.channel);
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

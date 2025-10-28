using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class ProjectileModifierProjectile : GlobalProjectile
{
	/// <summary>
	/// Tags a projectile spawned directly by a player (such as where <c>source is EntitySource_Parent { Entity: Player }</c>) as modifiable even if
	/// it's not a <see cref="EntitySource_ItemUse_WithAmmo"/>.
	/// </summary>
	internal const string SpeedUpProjectile = "Modifiable";

	private static readonly HashSet<int> InvalidProjectilesToMove = [];

	public override void SetStaticDefaults()
	{
		InvalidProjectilesToMove.Clear();
		InvalidProjectilesToMove.Add(ProjectileID.FinalFractal);
	}

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		Player player = null;
		bool valid = false;

		if (source is EntitySource_ItemUse_WithAmmo { Player: Player plr })
		{
			valid = true;
			player = plr;
		}

		if (source is EntitySource_Parent { Entity: Player plr2 } && source.Context is { } context && context.Contains(SpeedUpProjectile))
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
		}
	}
}
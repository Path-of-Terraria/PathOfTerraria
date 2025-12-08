using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives;

internal class FatBulletsMastery : Passive
{
	internal class FatBulletsProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private float _originalMagnitude = -1;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo ammo && ammo.Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<FatBulletsMastery>(out float value)
				&& ammo.Item.CountsAsClass(DamageClass.Ranged))
			{
				projectile.scale *= 1 + (value / 100f);
				projectile.damage = (int)(projectile.damage * (1 + (value / 100f)));
				projectile.velocity *= 5f;

				int extras = (int)(projectile.velocity.Length() / 16f);

				if (extras > 0)
				{
					projectile.extraUpdates += extras;
					projectile.velocity /= (float)extras;
				}

				_originalMagnitude = projectile.velocity.Length();
			}
		}

		public override bool PreAI(Projectile projectile)
		{
			if (_originalMagnitude != -1)
			{
				projectile.velocity *= 0.96f;
				projectile.Opacity = MathF.Min(1, projectile.velocity.Length() / 4f);
				
				if (projectile.velocity.LengthSquared() <= 0.1f)
				{
					projectile.Kill();
					return false;
				}
			}

			return true;
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.FinalDamage *= projectile.velocity.Length() / _originalMagnitude * 0.75f + 0.25f;
		}
	}
}
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class FatBulletsMastery : Passive
{
	internal class FatBulletsProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		private float _value;
		private string _ammoUsed;
		// Not required anymore to save the speed over functions
		//private float _originalMagnitude = -1;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo ammo && ammo.Player.GetModPlayer<PassiveTreePlayer>()
				                                                 .TryGetCumulativeValue<FatBulletsMastery>(
					                                                 out float value)
			                                                 && ammo.Item.CountsAsClass(DamageClass.Ranged))
			{
				_value = value;
				projectile.scale *=
					1 + (50 / 100f); // due to Vortex beater/Phantasm coding, this also scales the bow/gun size.
				//projectile.damage = (int)(projectile.damage * (1 + (value / 100f))); 
				projectile.velocity *=
					0.5f; //- This is actually not the cause of why Phantasm and Vortex beater go ballistic, only in combi with the function below this
				//_originalMagnitude = projectile.velocity.Length();


				// This makes it so that a bullet can at most live 60 ticks aka a second, this is roughly the amount necessary to make them fly as far as previously
				if (ammo.Item.useAmmo == AmmoID.Bullet)
				{
					projectile.timeLeft = 60;
					_ammoUsed = "Bullet";
				}

				if (ammo.Item.useAmmo == AmmoID.Arrow)
				{
					projectile.timeLeft = 90;
					_ammoUsed = "Arrow";
				}
			}
		}

		// this adds the (at the time of writing this) 30% damage modifier based on the remaining arrow lifetime, making it both not ask every tick and much more friendly to manage than the old one.
		// Arrows use 90 due to the inherently slower flight time 
		
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (_ammoUsed == "Bullet")
			{
				modifiers.FinalDamage *= projectile.timeLeft / 60 * _value / 100f + 1;
			}
			else if (_ammoUsed == "Arrow")
			{
				modifiers.FinalDamage *= projectile.timeLeft / 90 * _value / 100f + 1;
			}
		}

		// increasing the projectile.velocity in this function made my Vortex Beater and Phantasm fly forward in addition to making the projectile faster
/*
		public override bool PreAI(Projectile projectile)
		{
			if (_originalMagnitude != -1)
			{
				projectile.velocity *= 0.2f;
				projectile.Opacity = MathF.Min(1, projectile.velocity.Length() / 4f);

				if (projectile.velocity.LengthSquared() <= 0.15f)
				{
					projectile.Kill();
					return false;
				}
			}

			return true;
		}

// This causes bullets towards the end of the maximum reach to actually lose damage. It also does not do anything without the above function.

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (_originalMagnitude != -1)
			{
				modifiers.FinalDamage *= projectile.velocity.Length() / _originalMagnitude * 0.75f + 0.25f;
			}
		}
*/
	}
}
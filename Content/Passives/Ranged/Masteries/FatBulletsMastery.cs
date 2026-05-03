using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Ranged.Masteries;

internal class FatBulletsMastery : Passive
{
	internal class FatBulletsProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		private float _value;
		private string _ammoOld;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo ammo && ammo.Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<FatBulletsMastery>(out float value) && ammo.Item.CountsAsClass(DamageClass.Ranged))
			{
				_value = value;
				projectile.scale *= 1 + (50 / 100f); // due to Vortex beater/Phantasm coding, this also scales the bow/gun size.
				projectile.velocity *= 0.5f; // make the bullets feel heavier aka slow them down by 50%
				
				// This makes it so that a bullet can at most live 60 ticks aka a second, this is roughly the amount necessary to make them fly as far as previously
				if (ammo.Item.useAmmo == AmmoID.Bullet)
				{
					projectile.timeLeft = 60;
					_ammoOld = "Bullet";
				}

				if (ammo.Item.useAmmo == AmmoID.Arrow)
				{
					projectile.timeLeft = 90;
					_ammoOld = "Arrow";
				}
			}
		}
		// this adds the (at the time of writing this) 30% damage modifier based on the remaining arrow lifetime, making it both not ask every tick and much more friendly to manage than the old one.
		// Arrows use 90 due to the inherently slower flight time to reach the same distance as the old function.
		
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			Player projOwner = Main.player[projectile.owner];
			if (_ammoOld == "Bullet")
			{
				AdditiveScalingModifier.ApplyAdditiveLikeScalingProjectile(projOwner, projectile, ref modifiers, projectile.timeLeft / 60f * _value / 100f);
			}
			else if (_ammoOld == "Arrow")
			{
				AdditiveScalingModifier.ApplyAdditiveLikeScalingProjectile(projOwner, projectile, ref modifiers, projectile.timeLeft / 90f *  _value / 100f);
			}
		}
	}
}
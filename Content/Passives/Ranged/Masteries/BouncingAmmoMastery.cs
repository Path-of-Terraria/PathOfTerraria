using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class BouncingAmmoMastery : Passive
{
	internal class BouncingAmmoProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public int Bounces = 0;

		private ushort _lastNpcHit = ushort.MaxValue;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo ammo && ammo.Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BouncingAmmoMastery>(out float value) 
				&& ammo.AmmoItemIdUsed != AmmoID.None)
			{
				Bounces = (int)value;
			}
		}

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Bounces > 0)
			{
				int index = -1;
				float targetSqrDist = float.PositiveInfinity;
				Vector2 projectileCenter = projectile.Center;

				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.CanBeChasedBy() && npc.whoAmI != target.whoAmI
					&& npc.DistanceSQ(projectileCenter) is float dist and < 500 * 500 && dist < targetSqrDist)
					{
						index = npc.whoAmI;
						targetSqrDist = dist;
					}
				}

				if (index != -1)
				{
					_lastNpcHit = (ushort)target.whoAmI;

					projectile.penetrate++;
					projectile.velocity = projectile.DirectionTo(Main.npc[index].Center) * projectile.velocity.Length();
					projectile.damage = (int)(projectile.damage * 0.33f);
					projectile.netUpdate = true;
				}

				Bounces--;
			}
		}

		public override bool? CanHitNPC(Projectile projectile, NPC target)
		{
			return target.whoAmI != _lastNpcHit ? null : false;
		}
	}
}
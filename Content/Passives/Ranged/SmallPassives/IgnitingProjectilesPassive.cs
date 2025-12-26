using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class IgnitingProjectilesPassive : Passive
{
	internal class IgnitingProjectileProjectile : GlobalProjectile
	{
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (projectile.TryGetOwner(out Player plr) && plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<IgnitingProjectilesPassive>(out float value) 
				&& Main.rand.NextFloat() < value)
			{
				IgnitedDebuff.ApplyTo(plr, target, damageDone, 240);
			}
		}
	}
}
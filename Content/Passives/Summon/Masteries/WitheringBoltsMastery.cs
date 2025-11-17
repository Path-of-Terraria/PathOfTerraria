using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class WitheringBoltsMastery : Passive
{
	internal class WitheringBoltsProjectile : GlobalProjectile
	{
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!ProjectileID.Sets.SentryShot[projectile.type] || projectile.sentry)
			{
				return;
			}

			Player owner = Main.player[projectile.owner];

			if (owner.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<WitheringBoltsMastery>(out float value))
			{
				int duration = (int)owner.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<WitheringBoltsMastery>();
				target.AddBuff(ModContent.BuffType<WitheringBoltsDebuff>(), duration * 60);
			}
		}
	}
}
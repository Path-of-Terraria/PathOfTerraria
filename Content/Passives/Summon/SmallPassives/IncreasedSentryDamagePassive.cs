using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedSentryDamagePassive : Passive
{
	public sealed class IncreasedSentryDamagePassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			float value = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<IncreasedSentryDamagePassive>();

			if (projectile.sentry || ProjectileID.Sets.SentryShot[projectile.type])
			{
				Player projOwner = Main.player[projectile.owner];
				AdditiveScalingModifier.ApplyAdditiveLikeScalingProjectile(projOwner, projectile, ref modifiers, value / 100f);
			}
		}
	}
}
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedSentryKnockbackPassive : Passive
{
	public sealed class IncreasedSentryKnockbackPassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			float passiveValue = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<IncreasedSentryKnockbackPassive>();

			if (passiveValue > 0 && (proj.sentry || ProjectileID.Sets.SentryShot[proj.type]))
			{
				modifiers.Knockback *= 1 + (passiveValue / 100.0f);
			}
		}
	}
}
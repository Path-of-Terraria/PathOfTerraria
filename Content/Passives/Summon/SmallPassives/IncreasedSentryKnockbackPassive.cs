using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedSentryKnockbackPassive : Passive
{
	public sealed class IncreasedSentryKnockbackPassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			float level = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<IncreasedSentryKnockbackPassive>();

			if (level > 0 && (proj.sentry || ProjectileID.Sets.SentryShot[proj.type]))
			{
				float passiveValue = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<IncreasedSentryKnockbackPassive>();
				modifiers.Knockback *= 1 + (passiveValue / 100.0f);
			}
		}
	}
}
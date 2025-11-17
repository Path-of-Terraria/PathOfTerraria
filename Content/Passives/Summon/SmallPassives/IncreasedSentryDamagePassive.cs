using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedSentryDamagePassive : Passive
{
	public sealed class IncreasedSentryDamagePassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			float level = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<IncreasedSentryDamagePassive>();

			if (proj.sentry || ProjectileID.Sets.SentryShot[proj.type])
			{
				modifiers.FinalDamage += level / 100.0f;
			}
		}
	}
}
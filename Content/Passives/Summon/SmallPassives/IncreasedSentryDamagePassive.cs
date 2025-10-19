using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedSentryDamagePassive : Passive
{
	public sealed class IncreasedSentryDamagePassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			int level = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(Name);

			if (proj.sentry || ProjectileID.Sets.SentryShot[proj.type])
			{
				modifiers.FinalDamage += level * (ModContent.GetInstance<IncreasedSentryDamagePassive>().Value / 100.0f);
			}
		}
	}
}
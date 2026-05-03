using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedMinionDamagePassive : Passive
{
	public sealed class IncreasedMinionDamagePassivePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithProj(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			float value = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<IncreasedMinionDamagePassive>();

			if (projectile.minion)
			{
				Player projOwner = Main.player[projectile.owner];
				AdditiveScalingModifier.ApplyAdditiveLikeScalingProjectile(projOwner, projectile, ref modifiers, value / 100f);
			}
		}
	}
}

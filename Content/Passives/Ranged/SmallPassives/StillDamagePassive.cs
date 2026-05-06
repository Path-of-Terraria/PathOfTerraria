using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.Passives;

internal class StillDamagePassive : Passive
{
	internal class StillPlayer : GlobalNPC
	{
		// Allows Projectiles that hit as you stand still to do the 10% damage
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (projectile.TryGetOwner(out Player plr) && plr.velocity.LengthSquared() < 0.1f
				&& plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<StillDamagePassive>(out float value))
			{
				Player projOwner = Main.player[projectile.owner];
				AdditiveScalingModifier.ApplyAdditiveLikeScalingProjectile(projOwner, projectile, ref modifiers, value / 100f);
			}
			
		}
		// Allows standing still with a Melee weapon - this was previously not an option 
		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
		{
			if (player.velocity.LengthSquared() < 0.1f && player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<StillDamagePassive>(out float value))
			{
				AdditiveScalingModifier.ApplyAdditiveLikeScalingItem(player, item, ref modifiers, value / 100f);
			}
		}
	}
}
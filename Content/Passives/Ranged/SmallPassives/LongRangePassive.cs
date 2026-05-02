using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.Passives;

internal class LongRangePassive : Passive
{
	internal class LongRangeGlobalNPC : GlobalNPC
	{
		// Enables projectile outside of "DistanceSQ" to deal increased damage
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (projectile.TryGetOwner(out Player plr) && projectile.DistanceSQ(npc.Center) < PoTMod.NearbyDistanceSq
				&& plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<LongRangePassive>(out float value))
			{
				Player projOwner = Main.player[projectile.owner];
				AdditiveScalingModifier.ApplyAdditiveLikeScalingProjectile(projOwner, projectile, ref modifiers, value / 100f);
			}
		}
		
	}
}
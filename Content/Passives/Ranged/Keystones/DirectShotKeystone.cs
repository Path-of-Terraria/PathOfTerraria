using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Ranged.Keystones;

public class DirectShotKeystone : Passive
{
	internal class DirectShotProjectile : GlobalProjectile
	{
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (projectile.TryGetOwner(out Player player) && 
			    player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<DirectShotKeystone>(out float value) &&
			    projectile.friendly)
			{
				float distanceSquared = player.DistanceSQ(target.Center);

				if (distanceSquared <= PoTMod.NearbyDistanceSq * 4) 
				{
					float nearbyRatio = 1f - (distanceSquared / (PoTMod.NearbyDistanceSq * 4));
					float damageBonus = nearbyRatio * value/100f;
					modifiers.FinalDamage += damageBonus;
				}
				else
				{
					float farDistance = MathF.Sqrt(distanceSquared) - MathF.Sqrt(PoTMod.NearbyDistanceSq);
					float maxFarDistance = 1000f;
    
					float farRatio = MathF.Min(farDistance / maxFarDistance, 1f);
					float damagePenalty = farRatio * value/100f;
					modifiers.FinalDamage -= damagePenalty;
					
				}
			}
		}
	}
}
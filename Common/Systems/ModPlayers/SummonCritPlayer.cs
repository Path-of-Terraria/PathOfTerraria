using PathOfTerraria.Content.Projectiles.Summoner;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class SummonCritPlayer : ModPlayer
{
	public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (proj.IsMinionOrSentryRelated || proj.WhipSettings.Segments > 0)
		{
			float baseCritChance = Player.GetCritChance(DamageClass.Generic);
			float additionalCrit = Player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.SummonCritChance.Value * 100f;
			float totalCritChance = baseCritChance + additionalCrit;

			if (proj.ModProjectile is GrimoireSummon)
			{
				totalCritChance += Player.GetModPlayer<GrimoirePlayer>().Stats.CriticalStrikeChanceModifier.Value * 100;
			}
            
			if (Main.rand.NextFloat(100f) < totalCritChance)
			{
				modifiers.SetCrit();
			}
		}
	}
}

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class SummonCritPlayer : ModPlayer
{
	public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (proj.IsMinionOrSentryRelated && Player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.SummonCritChance.Value > Main.rand.NextFloat())
		{
			modifiers.SetCrit();
		}
	}
}

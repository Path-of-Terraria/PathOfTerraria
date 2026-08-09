using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// Unfinished.
/// </summary>
internal class SentryDamagePlayer : ModPlayer
{
	public StatModifier SentryDamage = default;

	public override void ResetEffects()
	{
		SentryDamage = default;
	}

	public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (proj.sentry || ProjectileID.Sets.SentryShot[proj.type])
		{
		}
	}
}

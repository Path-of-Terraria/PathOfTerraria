using PathOfTerraria.Common.Systems.TreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class MinionPassive : Passive
{
}

internal class SentryPassive : Passive
{
	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.ModifyHitNPCWithProjEvent += BuffSentries;
	}

	private void BuffSentries(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		int level = player.GetModPlayer<TreePlayer>().GetCumulativeLevel(InternalIdentifier);

		if (proj.sentry || ProjectileID.Sets.SentryShot[proj.type])
		{
			modifiers.FinalDamage += level * 0.1f;
		}
	}
}
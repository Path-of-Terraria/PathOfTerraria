using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class BlastChainMastery : Passive
{
	internal class BlastChainNPC : GlobalNPC
	{
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (projectile.TryGetOwner(out Player plr) && plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BlastChainMastery>(out float value)
				&& ProjectileID.Sets.Explosive[projectile.type] && npc.life <= 0)
			{
				ExplosionHitbox.QuickSpawn(npc.GetSource_Death(), npc, Vector2.Zero, (int)(hit.Damage * value / 100f), projectile.owner, npc.Size * 1.5f);
			}
		}
	}
}
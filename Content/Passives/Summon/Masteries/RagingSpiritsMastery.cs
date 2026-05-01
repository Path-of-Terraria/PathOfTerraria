using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class RagingSpiritsMastery : Passive
{
	internal class RagingSpiritsProjectile : GlobalProjectile
	{
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (projectile.minion && projectile.TryGetOwner(out Player owner) &&
			    owner.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<RagingSpiritsMastery>(out float value) &&
			    Main.rand.NextFloat() < value / 100f)
			{
				IEntitySource src = owner.GetSource_OnHit(target);
				Projectile.NewProjectile(src, target.Center, new Vector2(0, -14), ModContent.ProjectileType<RagingSpirit>(), damageDone / 2, 0, owner.whoAmI);
			}
		}
	}
}
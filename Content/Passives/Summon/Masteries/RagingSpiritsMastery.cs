using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class RagingSpiritsMastery : Passive
{
	internal class RagingSpiritsPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.DamageType.CountsAsClass(DamageClass.Summon) && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<RagingSpiritsMastery>(out float value)
				)//&& Main.rand.NextFloat() < value / 100f)
			{
				IEntitySource src = Player.GetSource_OnHit(target);
				Projectile.NewProjectile(src, target.Center, new Vector2(0, -14), ModContent.ProjectileType<RagingSpirit>(), damageDone, 0, Player.whoAmI);
			}
		}
	}
}
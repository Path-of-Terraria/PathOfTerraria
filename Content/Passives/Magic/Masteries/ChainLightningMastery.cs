using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class ChainLightningMastery : Passive
{
	internal class ChainLightningPlayer : ModPlayer, ElementalPlayerHooks.IElementalOnHitPlayer
	{
		public void ElementalOnHitNPC(NPC target, ElementInstance ele, ElementalContainer con, ElementalContainer other, int finalDamage, NPC.HitInfo hitInfo, Item item = null)
		{
			PassiveTreePlayer treePlayer = Player.GetModPlayer<PassiveTreePlayer>();

			if (ele.Type == ElementType.Lightning && treePlayer.TryGetCumulativeValue<ChainLightningMastery>(out float value) && Main.rand.NextFloat() < value / 100f)
			{
				Terraria.DataStructures.IEntitySource src = Player.GetSource_OnHit(target);
				Projectile.NewProjectile(src, target.Center, Vector2.Zero, ModContent.ProjectileType<ChainLightning>(), finalDamage, 0, Player.whoAmI, target.whoAmI);
			}
		}
	}
}

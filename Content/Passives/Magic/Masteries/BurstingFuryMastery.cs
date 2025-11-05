using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

#nullable enable

internal class BurstingFuryMastery : Passive
{
	internal class BurstingFuryPlayer : ModPlayer, ElementalPlayerHooks.IElementalOnHitPlayer
	{
		public void ElementalOnHitNPC(NPC target, ElementInstance ele, ElementalContainer con, ElementalContainer other, int finalDamage, NPC.HitInfo hitInfo, Item? item = null)
		{
			if (ele.Type == ElementType.Fire && finalDamage > 0 && target.life <= 0 && Player.GetModPlayer<PassiveTreePlayer>().HasNode<BurstingFuryMastery>())
			{
				int damage = (int)(finalDamage * Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<AfterburnMastery>() / 100f);
				int type = ModContent.ProjectileType<Flameburst>();
				Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, new Vector2(0, 0), type, damage, 8f, Player.whoAmI, 140, 140);
			}
		}
	}
}

using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class ReprisalMastery : Passive
{
	internal class ReprisalPlayer : ModPlayer, IOnBlockPlayer
	{
		public void OnBlock(Player.HurtInfo info)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ReprisalMastery>(out float value))
			{
				int damage = (int)(info.Damage * value / 100f);
				Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<SlashAoE>(), damage, 6, Player.whoAmI);
			}
		}
	}
}

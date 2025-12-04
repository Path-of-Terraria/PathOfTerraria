using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class BiggerExplosivesPassive : Passive
{
	internal class BiggerExplosivesProjectile : ILoadable
	{
		public void Load(Mod mod)
		{
			On_Projectile.Resize += ResizeExplosive;
		}

		private void ResizeExplosive(On_Projectile.orig_Resize orig, Projectile self, int newWidth, int newHeight)
		{
			if (ProjectileID.Sets.Explosive[self.type] && self.TryGetOwner(out Player player) 
				&& player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BiggerExplosivesPassive>(out float value))
			{
				newWidth = (int)(newWidth * (1 + value / 100f));
				newHeight = (int)(newHeight * (1 + value / 100f));
			}

			orig(self, newWidth, newHeight);
		}

		public void Unload()
		{
		}
	}
}
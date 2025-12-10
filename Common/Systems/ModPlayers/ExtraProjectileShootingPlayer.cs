using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class ExtraProjectileShootingPlayer : ModPlayer
{
	public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (Player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ExtraProjectilesShot is { Value: >0 } shots)
		{
			float count = shots.Value;

			while (count > 0)
			{
				if (count >= 1)
				{
					Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Player.whoAmI);
				}
				else if (Main.rand.NextFloat() < count)
				{
					Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Player.whoAmI);
				}

				count--;
			}
		}

		return true;
	}
}

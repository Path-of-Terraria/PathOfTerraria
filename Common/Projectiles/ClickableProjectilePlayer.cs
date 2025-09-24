namespace PathOfTerraria.Common.Projectiles;

/// <summary>
/// Allows a given projectile to have right click functionality.
/// </summary>
internal interface IRightClickableProjectile
{
	/// <summary>
	/// Called when the projectile is right clicked on the local client.
	/// </summary>
	public bool RightClick(Projectile self, Player player);
}

/// <summary>
/// Allows the developer to define behaviour when the player hovers over a specific projectile.<br/>
/// Runs only on the client.
/// </summary>
internal class ClickableProjectilePlayer : ModPlayer
{
	public override void UpdateEquips()
	{
		if (Main.myPlayer == Player.whoAmI)
		{
			foreach (Projectile projectile in Main.ActiveProjectiles)
			{
				if (projectile.ModProjectile is IRightClickableProjectile right && projectile.Hitbox.Contains(Main.MouseWorld.ToPoint())
					&& Player.IsInTileInteractionRange(Player.tileTargetX, Player.tileTargetY, Terraria.DataStructures.TileReachCheckSettings.Simple))
				{
					if (right.RightClick(projectile, Player))
					{
						return;
					}
				}
			}
		}
	}
}

using System.Collections.Generic;

namespace PathOfTerraria.Common.Projectiles;

/// <summary>
/// Allows the developer to define behaviour when the player hovers over a specific projectile.<br/>
/// Runs only on the client.
/// </summary>
internal class ClickableProjectilePlayer : ModPlayer
{
	private readonly static Dictionary<int, Action<Projectile, Player>> OnHoverProjectile = [];

	/// <summary>
	/// Registers a projectile ID and a corresponding hover action to the internal dictionary. This should be run in <see cref="ModType.SetStaticDefaults"/>.
	/// </summary>
	/// <param name="projectile">Projectile ID to use.</param>
	/// <param name="onClick">Behaviour to run for the projectile. Takes in the current projectile and the client.</param>
	public static void RegisterProjectile(int projectile, Action<Projectile, Player> onClick)
	{
		if (Main.dedServ)
		{
			return;
		}

		OnHoverProjectile.Add(projectile, onClick);
	}

	public override void UpdateEquips()
	{
		if (Main.myPlayer == Player.whoAmI)
		{
			foreach (Projectile projectile in Main.ActiveProjectiles)
			{
				if (OnHoverProjectile.TryGetValue(projectile.type, out Action<Projectile, Player> value) && projectile.Hitbox.Contains(Main.MouseWorld.ToPoint())
					&& Player.IsInTileInteractionRange(Player.tileTargetX, Player.tileTargetY, Terraria.DataStructures.TileReachCheckSettings.Simple))
				{
					value.Invoke(projectile, Player);
				}
			}
		}
	}
}

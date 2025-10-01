using Terraria.GameInput;
using Terraria.ID;

namespace PathOfTerraria.Common.Projectiles;

/// <summary>
/// Allows a given projectile to have right click functionality.
/// </summary>
internal interface IRightClickableProjectile
{
	public Projectile Projectile { get; }

	/// <summary>
	/// Called when the projectile is right clicked on the local client.
	/// </summary>
	public bool RightClick(Player player, bool mouseDirectlyOver);
}

/// <summary>
/// Allows the developer to define behaviour when the player hovers over a specific projectile.<br/>
/// Runs only on the client.
/// </summary>
internal static class RightClickableProjectileExtensions
{
	internal static int TryInteracting(this IRightClickableProjectile rightClickableProjectile)
	{
		if (Main.gamePaused || Main.gameMenu)
		{
			return 0;
		}

		bool cursorHighlights = Main.SmartCursorIsUsed || PlayerInput.UsingGamepad;
		Player localPlayer = Main.LocalPlayer;
		Vector2 compareSpot = localPlayer.Center;
		Projectile projectile = rightClickableProjectile.Projectile;

		if (!localPlayer.IsProjectileInteractibleAndInInteractionRange(projectile, ref compareSpot))
		{
			return 0;
		}

		// Due to a quirk in how projectiles drawn using behindProjectiles are implemented,
		// we need to do some math to calculate the correct world position of the mouse instead of using Main.MouseWorld directly.
		var matrix = Matrix.Invert(Main.GameViewMatrix.ZoomMatrix);
		Vector2 position = Main.ReverseGravitySupport(Main.MouseScreen);
		Vector2.Transform(Main.screenPosition, matrix);
		Vector2 realMouseWorld = Vector2.Transform(position, matrix) + Main.screenPosition;

		bool mouseDirectlyOver = projectile.Hitbox.Contains(realMouseWorld.ToPoint());
		bool interactingWithThisProjectile = mouseDirectlyOver || Main.SmartInteractProj == projectile.whoAmI;

		if (!interactingWithThisProjectile || localPlayer.lastMouseInterface)
		{
			return cursorHighlights ? 1 : 0; // 0 == Don't draw highlight texture, 1 == Draw faded highlight
		}

		Main.HasInteractibleObjectThatIsNotATile = true;

		if (PlayerInput.UsingGamepad)
		{
			localPlayer.GamepadEnableGrappleCooldown();
		}

		if (Player.BlockInteractionWithProjectiles == 0 && rightClickableProjectile.RightClick(Main.LocalPlayer, mouseDirectlyOver))
		{
			localPlayer.tileInteractAttempted = true;
			localPlayer.tileInteractionHappened = true;
			localPlayer.releaseUseTile = false;
			Main.mouseRightRelease = false;
		}

		if (cursorHighlights)
		{
			return 2; // Draw highlight texture
		}
	
		return 0;
	}

	internal static void DrawHighlightAndCheckRightClickInteraction(this IRightClickableProjectile clickProj, Texture2D tex, Vector2 position, Color lightColor, float scale = 1f)
	{
		int highlightMode = clickProj.TryInteracting();

		if (highlightMode == 0)
		{
			return;
		}

		int lightValue = (lightColor.R + lightColor.G + lightColor.B) / 3;
		bool isProjectileSelected = highlightMode == 2;
		Color highlightColor = Colors.GetSelectionGlowColor(isProjectileSelected, lightValue);
		Main.spriteBatch.Draw(tex, position, null, highlightColor, clickProj.Projectile.rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0);
	}
}

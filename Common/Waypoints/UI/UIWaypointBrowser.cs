using PathOfTerraria.Common.Utilities;
using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointBrowser : UIState
{
	public const string Identifier = $"{PoTMod.ModName}:{nameof(UIWaypointBrowser)}";

	/// <summary>
	///		The position of the arcane obelisk instance in tile coordinates.
	/// </summary>
	public readonly Point Coordinates;

	private float _progress;

	public float Progress
	{
		get => _progress;
		set => _progress = MathHelper.Clamp(_progress, 0f, 1f);
	}

	private bool activated;

	public UIWaypointBrowser(Point coordinates)
	{
		Coordinates = coordinates;
	}

	public override void OnActivate()
	{
		base.OnActivate();

		Progress = 0f;
		
		activated = true;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();

		activated = false;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		_progress = MathHelper.SmoothStep(_progress, activated ? 1f : 0f, 0.2f);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		var tileOffset = new Vector2(1, 2) * 16f + new Vector2(TileUtils.TilePixelSize / 2f);
		var heightOffset = new Vector2(0f, 64f * Progress);
		
		var center = new Vector2(Coordinates.X, Coordinates.Y) * 16f - Main.screenPosition - tileOffset - heightOffset;

		float horizontalOffset = 0f;
		
		foreach (ModWaypoint waypoint in ModContent.GetContent<ModWaypoint>())
		{
			Asset<Texture2D> icon = ModContent.Request<Texture2D>(waypoint.IconPath, AssetRequestMode.ImmediateLoad);
			
			spriteBatch.Draw(
				icon.Value,
				center + new Vector2(horizontalOffset, 0f),
				Color.White * Progress
			);

			horizontalOffset += icon.Width() + 4f;
		}
	}
}
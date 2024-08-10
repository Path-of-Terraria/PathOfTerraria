using PathOfTerraria.Common.Utilities;
using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointBrowser : UIState
{
	/// <summary>
	///		The unique identifier of this state.
	/// </summary>
	public const string Identifier = $"{PoTMod.ModName}:{nameof(UIWaypointBrowser)}";

	/// <summary>
	///		The position of the arcane obelisk instance in tile coordinates.
	/// </summary>
	public readonly Point Coordinates;

	private float _progress;

	/// <summary>
	///		The animation progress of the browser.
	/// </summary>
	/// <remarks>
	///		This ranges from <c>0</c> (Inactive) - <c>1</c> (Active).
	/// </remarks>
	public float Progress
	{
		get => _progress;
		set => _progress = MathHelper.Clamp(value, 0f, 1f);
	}

	private float _targetProgress;
	
	/// <summary>
	///		The target value for the animation progress of the browser.
	/// </summary>
	/// <remarks>
	///		This ranges from <c>0</c> (Inactive) - <c>1</c> (Active).
	/// </remarks>
	public float TargetProgress 
	{
		get => _targetProgress;
		set => _targetProgress = MathHelper.Clamp(value, 0f, 1f);
	}
	
	public UIWaypointBrowser(Point coordinates)
	{
		Coordinates = coordinates;
	}

	public override void OnActivate()
	{
		base.OnActivate();

		TargetProgress = 1f;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();

		TargetProgress = 0f;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		Progress = MathHelper.SmoothStep(Progress, TargetProgress, 0.2f);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		var tileOffset = new Vector2(1, 2) * 16f + new Vector2(TileUtils.TilePixelSize / 2f);
		var heightOffset = new Vector2(0f, 64f * Progress);
		
		var position = new Vector2(Coordinates.X, Coordinates.Y) * 16f - Main.screenPosition - tileOffset - heightOffset;

		for (int i = 0; i < ModWaypointLoader.Waypoints.Count; i++)
		{
			ModWaypoint waypoint = ModWaypointLoader.Waypoints[i];
			
			Asset<Texture2D> icon = ModContent.Request<Texture2D>(waypoint.IconPath, AssetRequestMode.ImmediateLoad);
			
			// TODO: Implement drawing logic.
		}
	}
}
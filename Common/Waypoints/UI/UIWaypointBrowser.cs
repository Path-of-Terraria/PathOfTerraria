using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Utilities;
using ReLogic.Content;
using Terraria.GameInput;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointBrowser : UIState
{
	private const int KeyInitialDelay = 30;
	private const int KeyRepeatDelay = 2;
	
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

	private int _selectedWaypoint;

	/// <summary>
	///		The index of the currently selected waypoint.
	/// </summary>
	/// <remarks>
	///		This will be set to the first element index if it goes out of bounds positively,
	///		and set to the last element index if it goes out of bounds negatively.
	/// </remarks>
	public int SelectedWaypoint
	{
		get => _selectedWaypoint;
		set
		{
			int min = 0;
			int max = ModWaypointLoader.WaypointCount;
			
			if (value < min)
			{
				_selectedWaypoint = max;
			}
			else if (value > max)
			{
				_selectedWaypoint = min;
			}
			else
			{
				_selectedWaypoint = (int)MathHelper.Clamp(value, min, max);
			}
		}
	}

	private int inputTimer;
	
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

		UpdateWaypointSelection();
		
		Progress = MathHelper.SmoothStep(Progress, TargetProgress, 0.2f);
	}

	private void UpdateWaypointSelection()
	{
		if (Main.keyState.IsKeyDown(Keys.Right))
		{
			if (inputTimer == 0 || (inputTimer > KeyInitialDelay && inputTimer % 5 == 0))
			{
				SelectedWaypoint++;
			}

			inputTimer++;
		}
		else if (Main.keyState.IsKeyDown(Keys.Left))
		{
			if (inputTimer == 0 || (inputTimer > KeyInitialDelay && inputTimer % 5 == 0))
			{
				SelectedWaypoint--;
			}

			inputTimer++;
		}
		else if (PlayerInput.ScrollWheelDeltaForUI != 0)
		{
			if (inputTimer == 0 || (inputTimer > KeyInitialDelay && inputTimer % 5 == 0))
			{
				SelectedWaypoint += Math.Sign(PlayerInput.ScrollWheelDeltaForUI);
			}

			inputTimer++;
		}
		else
		{
			inputTimer = 0;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		const float IconOffset = 48f;
		
		var tileOffset = new Vector2(1, 2) * 16f + new Vector2(TileUtils.TilePixelSize / 2f);
		var heightOffset = new Vector2(0f, 80f * Progress);
		
		var drawPosition = new Vector2(Coordinates.X, Coordinates.Y) * 16f - Main.screenPosition - tileOffset - heightOffset;
		
		for (int i = 0; i < ModWaypointLoader.WaypointCount; i++)
		{
			ModWaypoint waypoint = ModWaypointLoader.Waypoints[i];

			Asset<Texture2D> icon = ModContent.Request<Texture2D>(waypoint.IconPath, AssetRequestMode.ImmediateLoad);

			if (i == SelectedWaypoint)
			{
				spriteBatch.Draw(icon.Value, drawPosition + icon.Size() / 2f, Color.White * Progress);
			}
			else if (i == SelectedWaypoint - 1)
			{
				spriteBatch.Draw(icon.Value, drawPosition + icon.Size() / 2f - new Vector2(IconOffset, 0f), Color.White * 0.4f * Progress);
			}
			else if (i == SelectedWaypoint + 1)
			{
				spriteBatch.Draw(icon.Value, drawPosition + icon.Size() / 2f + new Vector2(IconOffset, 0f), Color.White * 0.4f * Progress);
			}
		}
	}
}
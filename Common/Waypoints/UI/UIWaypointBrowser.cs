using Microsoft.Xna.Framework.Input;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointBrowser : UIState
{
	private const int KeyInitialDelay = 30;
	private const int KeyRepeatDelay = 15;

	/// <summary>
	///     The unique identifier of this state.
	/// </summary>
	public const string Identifier = $"{PoTMod.ModName}:{nameof(UIWaypointBrowser)}";

	/// <summary>
	///     The width of this state in pixels.
	/// </summary>
	public const float FullWidth = 300f;

	/// <summary>
	///     The height of this state in pixels.
	/// </summary>
	public const float FullHeight = 400f;

	/// <summary>
	///     The animation progress of this state.
	/// </summary>
	/// <remarks>
	///     This ranges from <c>0f</c> (Inactive) - <c>1f</c> (Active).
	/// </remarks>
	public float Progress
	{
		get => _progress;
		set => _progress = MathHelper.Clamp(value, 0f, 1f);
	}

	/// <summary>
	///     The target value for the animation progress of this state.
	/// </summary>
	/// <remarks>
	///     This ranges from <c>0f</c> (Inactive) - <c>1f</c> (Active).
	/// </remarks>
	public float TargetProgress
	{
		get => _targetProgress;
		set => _targetProgress = MathHelper.Clamp(value, 0f, 1f);
	}

	/// <summary>
	///     The index of the currently selected waypoint.
	/// </summary>
	public int SelectedWaypoint
	{
		get => _selectedWaypoint;
		set => _selectedWaypoint = (int)MathHelper.Clamp(value, 0, ModWaypointLoader.WaypointCount - 1);
	}

	private float _progress;

	private int _selectedWaypoint;

	private float _targetProgress;

	private int holdDelayTimer;

	private UIImage previewIndicator;

	public override void OnInitialize()
	{
		base.OnInitialize();

		var root = new UIElement
		{
			HAlign = 0.5f
		};

		root.Width.Set(FullWidth, 0f);
		root.Height.Set(FullHeight, 0f);

		Append(root);

		var panel = new UIPanel(
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBackground"),
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBorder"),
			13
		)
		{
			BackgroundColor = new Color(41, 66, 133) * 0.8f,
			BorderColor = new Color(13, 13, 15),
			OverrideSamplerState = SamplerState.PointClamp
		};

		panel.Width.Set(FullWidth, 0f);
		panel.Height.Set(FullHeight, 0f);

		root.Append(panel);

		var list = new UIList { OverrideSamplerState = SamplerState.PointClamp };

		list.Top.Set(50f, 0f);

		list.Width.Set(FullWidth, 0f);
		list.Height.Set(FullHeight - list.Top.Pixels, 0f);

		list.ListPadding = 0f;

		root.Append(list);

		var scroll = new UIScrollbar { OverrideSamplerState = SamplerState.PointClamp };

		scroll.Left.Set(FullWidth - 16f, 0f);

		scroll.Width.Set(20f, 0f);
		scroll.Height.Set(list.Height.Pixels, 0f);

		list.Append(scroll);
		list.SetScrollbar(scroll);

		for (int i = 0; i < ModWaypointLoader.WaypointCount; i++)
		{
			ModWaypoint? waypoint = ModWaypointLoader.Waypoints[i];
			
			var tab = new UIWaypointTab(waypoint, i);

			tab.Left.Set(12f, 0f);

			tab.Height.Set(48f, 0f);
			tab.Width.Set(FullWidth - 38f, 0f);

			tab.OnUpdate += _ => tab.Selected = tab.Index == SelectedWaypoint;

			list.Add(tab);
		}
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

		UpdateSelectionInput();
		
		Progress = MathHelper.SmoothStep(Progress, TargetProgress, 0.3f);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
	}

	private void UpdateSelectionInput()
	{
		if (Main.keyState.IsKeyDown(Keys.Down))
		{
			if (holdDelayTimer == 0 || holdDelayTimer > KeyInitialDelay && holdDelayTimer % KeyRepeatDelay == 0)
			{
				SelectedWaypoint++;
			}

			holdDelayTimer++;
		}
		else if (Main.keyState.IsKeyDown(Keys.Up))
		{
			if (holdDelayTimer == 0 || holdDelayTimer > KeyInitialDelay && holdDelayTimer % KeyRepeatDelay == 0)
			{
				SelectedWaypoint--;
			}

			holdDelayTimer++;
		}
		else if (PlayerInput.ScrollWheelDeltaForUI != 0)
		{
			if (holdDelayTimer == 0 || holdDelayTimer > KeyInitialDelay && holdDelayTimer % KeyRepeatDelay == 0)
			{
				SelectedWaypoint -= Math.Sign(PlayerInput.ScrollWheelDeltaForUI);
			}

			holdDelayTimer++;
		}
		else
		{
			holdDelayTimer = 0;
		}
	}
}
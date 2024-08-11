using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Common.Utilities;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointBrowser : UIState
{
	private const int KeyInitialDelay = 30;
	private const int KeyRepeatDelay = 15;
	
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
			int max = ModWaypointLoader.WaypointCount - 1;
			
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

	public override void OnInitialize()
	{
		base.OnInitialize();

		var root = new UIElement
		{
			HAlign = 0.5f,
			VAlign = 0.5f
		};
		
		root.Width.Set(400f, 0f);
		root.Height.Set(600f, 0f);

		Append(root);

		var panel = new UIPanel();
		
		panel.Width.Set(400f, 0f);
		panel.Height.Set(600f, 0f);
		
		root.Append(panel);

		var header = new UIText("Waypoints")
		{
			HAlign = 0.5f
		};
		
		header.Top.Set(16f, 0f);
		
		root.Append(header);

		var list = new UIList();
		
		list.Top.Set(32f, 0f);
		
		list.Width.Set(400f, 0f);
		list.Height.Set(600f - 32f, 0f);

		foreach (ModWaypoint? waypoint in ModContent.GetContent<ModWaypoint>())
		{
			var element = new UIElement();
			
			element.Width.Set(400f, 0f);
			element.Height.Set(48f, 0f);

			var icon = new UIHoverImage(ModContent.Request<Texture2D>(waypoint.IconPath, AssetRequestMode.ImmediateLoad))
			{
				VAlign = 0.5f,
				ActiveScale = 1.25f,
				InactiveScale = 1f,
				ActiveRotation = MathHelper.ToRadians(2.5f),
				InactiveRotation = 0f,
				OverrideSamplerState = SamplerState.PointClamp
			};
			
			icon.Left.Set(16f, 0f);
			
			element.Append(icon);

			var text = new UIText(waypoint.DisplayName.Value, 0.8f) { VAlign = 0.5f };
			
			text.Left.Set(64f, 0f);
			
			element.Append(text);
			
			list.Add(element);
		}
		
		list.ListPadding = 4f;

		root.Append(list);

		var scrollbar = new UIScrollbar();
		
		scrollbar.Width.Set(8f, 0f);
		scrollbar.Height.Set(0f, 0.825f);
		
		scrollbar.Left.Set(-16f, 1f);
		scrollbar.Top.Set(0f, 0.1f);
		
		list.Append(scrollbar);
		list.SetScrollbar(scrollbar);
	}

	public override void OnActivate()
	{
		base.OnActivate();
		
		OnInitialize();
		
		TargetProgress = 1f;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
RemoveAllChildren();
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
	}
}
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointList : UIElement
{
	private const int KeyInitialDelay = 30;
	private const int KeyRepeatDelay = 15;

	public const float Padding = 4f;
	
	/// <summary>
	///     The width of this element's panel in pixels.
	/// </summary>
	public const float PanelWidth = 280f;

	/// <summary>
	///		The full width of this element in pixels.
	/// </summary>
	public const float FullWidth = PanelWidth + 20f + Padding;

	/// <summary>
	///     The height of this element in pixels.
	/// </summary>
	public const float FullHeight = 400f;
	
	/// <summary>
	///     The index of the currently selected waypoint.
	/// </summary>
	public int SelectedWaypointIndex
	{
		get => _selectedWaypointIndex;
		set => _selectedWaypointIndex = (int)MathHelper.Clamp(value, 0, ModWaypointLoader.WaypointCount - 1);
	}

	public UIWaypointTab SelectedTab => tabs[SelectedWaypointIndex];
	
	private int _selectedWaypointIndex;
	
	private int holdDelayTimer;

	private List<UIWaypointTab> tabs = new();
	
	public override void OnInitialize()
	{
		base.OnInitialize();
		
		Width.Set(FullWidth, 0f);
		Height.Set(FullHeight, 0f);
		
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

		panel.Width.Set(PanelWidth, 0f);
		panel.Height.Set(FullHeight, 0f);

		Append(panel);

		var list = new UIList { OverrideSamplerState = SamplerState.PointClamp };

		list.Top.Set(50f, 0f);

		list.Width.Set(PanelWidth, 0f);
		list.Height.Set(FullHeight - list.Top.Pixels, 0f);

		list.ListPadding = 0f;

		Append(list);

		var scroll = new UIScrollbar { OverrideSamplerState = SamplerState.PointClamp };

		scroll.Left.Set(PanelWidth - 16f, 0f);

		scroll.Width.Set(20f, 0f);
		scroll.Height.Set(list.Height.Pixels, 0f);

		list.Append(scroll);
		list.SetScrollbar(scroll);

		for (int i = 0; i < ModWaypointLoader.WaypointCount; i++)
		{
			ModWaypoint? waypoint = ModWaypointLoader.Waypoints[i];
			
			var tab = new UIWaypointTab(waypoint, i);

			tab.Left.Set(2f, 0f);

			tab.Height.Set(48f, 0f);
			
			// Hardcoded value to make it centered relative to the scrollbar.
			tab.Width.Set(PanelWidth - 18f, 0f);

			tab.OnUpdate += _ => tab.Selected = tab.Index == SelectedWaypointIndex;

			list.Add(tab);
			
			tabs.Add(tab);
		}

		var indicator = new UIHoverImage(
			ModContent.Request<Texture2D>(
				$"{PoTMod.ModName}/Assets/UI/Inventory/Button_Right",
				AssetRequestMode.ImmediateLoad
			)
		)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			Color = Color.White * 0.8f,
			ActiveScale = 1.25f
		};
		
		indicator.Left.Set(PanelWidth + Padding, 0f);

		indicator.OnUpdate += (_) =>
		{
			indicator.Top.Pixels = MathHelper.SmoothStep(
				indicator.Top.Pixels,
				list.Top.Pixels + SelectedTab.Top.Pixels + 10f,
				0.3f
			);
		};
		
		Append(indicator);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		
		UpdateSelectionInput();
	}

	private void UpdateSelectionInput()
	{
		if (Main.keyState.IsKeyDown(Keys.Down))
		{
			if (holdDelayTimer == 0 || holdDelayTimer > KeyInitialDelay && holdDelayTimer % KeyRepeatDelay == 0)
			{
				SelectedWaypointIndex++;
			}

			holdDelayTimer++;
		}
		else if (Main.keyState.IsKeyDown(Keys.Up))
		{
			if (holdDelayTimer == 0 || holdDelayTimer > KeyInitialDelay && holdDelayTimer % KeyRepeatDelay == 0)
			{
				SelectedWaypointIndex--;
			}

			holdDelayTimer++;
		}
		else if (PlayerInput.ScrollWheelDeltaForUI != 0)
		{
			if (holdDelayTimer == 0 || holdDelayTimer > KeyInitialDelay && holdDelayTimer % KeyRepeatDelay == 0)
			{
				SelectedWaypointIndex -= Math.Sign(PlayerInput.ScrollWheelDeltaForUI);
			}

			holdDelayTimer++;
		}
		else
		{
			holdDelayTimer = 0;
		}
	}
}
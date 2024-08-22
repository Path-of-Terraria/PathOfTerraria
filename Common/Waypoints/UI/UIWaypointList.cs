using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointList : UIElement
{
	private const int KeyInitialDelay = 30;
	private const int KeyRepeatDelay = 15;

	/// <summary>
	///		The vertical margin of this element in pixels.
	/// </summary>
	public const float VerticalMargin = 50f;

	/// <summary>
	///     The width of this element in pixels.
	/// </summary>
	public const float FullWidth = 280f;

	/// <summary>
	///		The height of this element in pixels.
	/// </summary>
	public const float FullHeight = UIWaypointBrowser.FullHeight;

	/// <summary>
	///     The index of the currently selected waypoint tab.
	/// </summary>
	public int SelectedWaypointIndex
	{
		get => _selectedWaypointIndex;
		set => _selectedWaypointIndex = (int)MathHelper.Clamp(value, 0, ModWaypointLoader.WaypointCount - 1);
	}

	/// <summary>
	///		The instance of the currently selected waypoint tab.
	/// </summary>
	public UIWaypointTab SelectedTab => tabs[SelectedWaypointIndex];
	
	private int _selectedWaypointIndex;
	
	private int holdDelayTimer;

	private List<UIWaypointTab> tabs = new();
	
	public override void OnInitialize()
	{
		base.OnInitialize();
		
		Width.Set(FullWidth, 0f);
		Height.Set(FullHeight, 0f);

		Append(BuildPanel());

		var list = BuildList();
		
		PopulateList(list);
		Append(list);
	}
	
	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		
		UpdateInput();
	}

	private UIPanel BuildPanel()
	{
		var panel = new UIPanel(
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBackground"),
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBorder"),
			13
		)
		{
			BackgroundColor = new Color(41, 66, 133) * 0.8f,
			BorderColor = new Color(13, 13, 15),
			OverrideSamplerState = SamplerState.PointClamp,
			Width = { Pixels = FullWidth },
			Height = { Pixels = FullHeight }
		};

		return panel;
	}

	private UIList BuildList()
	{
		var list = new UIList
		{
			OverrideSamplerState = SamplerState.PointClamp,
			ListPadding = 0f,
			Top = { Pixels = VerticalMargin },
			Width = { Pixels = FullWidth },
			Height = { Pixels = FullHeight - VerticalMargin }
		};

		Append(list);

		var scroll = new UIScrollbar
		{
			OverrideSamplerState = SamplerState.PointClamp,
			Left = { Pixels = FullWidth - 16f },
			Width = { Pixels = 20f },
			Height = { Pixels = FullHeight - VerticalMargin }
		};

		list.Append(scroll);
		list.SetScrollbar(scroll);

		return list;
	}

	private void PopulateList(UIList list)
	{
		for (int i = 0; i < ModWaypointLoader.WaypointCount; i++)
		{
			ModWaypoint? waypoint = ModWaypointLoader.Waypoints[i];
			
			Asset<Texture2D>? icon = ModContent.Request<Texture2D>(waypoint.IconPath, AssetRequestMode.ImmediateLoad);

			var tab = new UIWaypointTab(icon, waypoint.DisplayName, i) { Left = { Pixels = 2f } };

			tab.OnUpdate += _ => tab.Selected = tab.Index == SelectedWaypointIndex;

			list.Add(tab);
			
			// We keep track of the tabs separately from the list so we can provide the currently selected tab.
			tabs.Add(tab);
		}
	}

	private void UpdateInput()
	{
		if (Main.keyState.IsKeyDown(Keys.Down))
		{
			ProcessInput(1);
		}
		else if (Main.keyState.IsKeyDown(Keys.Up))
		{
			ProcessInput(-1);
		}
		else if (PlayerInput.ScrollWheelDeltaForUI != 0)
		{
			ProcessInput(-Math.Sign(PlayerInput.ScrollWheelDeltaForUI));
		}
		else
		{
			holdDelayTimer = 0;
		}
	}

	private void ProcessInput(int direction)
	{
		if (!CanProcessInput() || !ContainsPoint(Main.MouseScreen))
		{
			return;
		}
		
		SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = 0.25f,
				MaxInstances = 1
			}
		);

		holdDelayTimer++;

		SelectedWaypointIndex += direction;
	}
	
	private bool CanProcessInput()
	{
		return holdDelayTimer == 0 || holdDelayTimer > KeyInitialDelay && holdDelayTimer % KeyRepeatDelay == 0;
	}
}
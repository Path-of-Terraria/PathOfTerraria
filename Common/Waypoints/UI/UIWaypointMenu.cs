using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointMenu : UIState
{
	/// <summary>
	///     The unique identifier of this state.
	/// </summary>
	public const string Identifier = $"{PoTMod.ModName}:{nameof(UIWaypointMenu)}";

	public const float FullWidth = ListFullWidth + InfoFullWidth + ElementPadding;
	public const float FullHeight = UIWaypointListElement.FullHeight * 5f + HeaderMargin;

	private const int KeyInitialDelay = 30;
	private const int KeyRepeatDelay = 15;

	public const float HeaderMargin = 48f;

	public const float ListFullWidth = 180f;
	public const float ListFullHeight = FullHeight;

	public const float InfoFullWidth = 360f;
	public const float InfoFullHeight = FullHeight;

	public const float ElementPadding = 12f;

	public int SelectedWaypointIndex
	{
		get => _selectedWaypointIndex;
		set => _selectedWaypointIndex = (int)MathHelper.Clamp(value, 0, ModWaypointLoader.WaypointCount - 1);
	}

	public UIWaypointListElement SelectedListElement => tabs[SelectedWaypointIndex];
	public ModWaypoint SelectedListWaypoint => ModWaypointLoader.Waypoints[SelectedWaypointIndex];

	private readonly List<UIWaypointListElement> tabs = [];

	private int _selectedWaypointIndex;

	public bool Enabled;

	private int holdDelayTimer;

	private UIImage thumbnailImage;
	private UIText waypointHeader;

	public override void OnInitialize()
	{
		base.OnInitialize();

		var root = new UIElement
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			Top = { Pixels = Main.screenHeight },
			Width = { Pixels = FullWidth },
			Height = { Pixels = FullHeight }
		};

		root.OnUpdate += RootUpdateEvent;

		root.Append(BuildPanel());

		Append(root);

		var listRoot = new UIElement
		{
			Width = { Pixels = ListFullWidth },
			Height = { Pixels = ListFullHeight }
		};

		listRoot.PaddingLeft = 8f;
		listRoot.PaddingRight = 8f;

		root.Append(listRoot);

		var header = new UIText("Waypoints", 1.2f)
		{
			HAlign = 0.5f,
			Top = { Pixels = HeaderMargin / 4f }
		};

		listRoot.Append(header);

		UIList list = BuildList();

		listRoot.Append(list);

		var panel = new UIPanel(
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBackground"),
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBorder"),
			13
		)
		{
			BackgroundColor = new Color(68, 97, 175) * 0.8f,
			BorderColor = Color.White * 0.8f,
			OverrideSamplerState = SamplerState.PointClamp,
			Width = { Pixels = FullWidth },
			Height = { Pixels = 48f },
			VAlign = 1f,
			Top = { Pixels = -8f }
		};

		listRoot.Append(panel);

		var text = new UIText("Travel")
		{
			HAlign = 0.5f,
			VAlign = 0.5f
		};

		panel.Append(text);

		var separator = new UIImage(TextureAssets.MagicPixel)
		{
			ScaleToFit = true,
			VAlign = 0.5f,
			Left = { Pixels = ListFullWidth + 1f },
			Width = { Pixels = 2f },
			Height = { Pixels = FullHeight - 32f },
			Color = Color.White * 0.5f
		};

		root.Append(separator);

		var infoRoot = new UIElement
		{
			Left = { Pixels = ListFullWidth + ElementPadding },
			Width = { Pixels = InfoFullWidth },
			Height = { Pixels = InfoFullHeight }
		};

		infoRoot.PaddingRight = 8f;

		root.Append(infoRoot);

		thumbnailImage = new UIImage(ModContent.Request<Texture2D>(SelectedListWaypoint.PreviewPath, AssetRequestMode.ImmediateLoad))
		{
			ScaleToFit = true,
			Top = { Pixels = HeaderMargin },
			Width = { Pixels = InfoFullWidth },
			Height = { Pixels = InfoFullHeight - HeaderMargin - 8f },
			OverrideSamplerState = SamplerState.PointClamp
		};

		infoRoot.Append(thumbnailImage);

		waypointHeader = new UIText(SelectedListWaypoint.DisplayName, 1.2f)
		{
			HAlign = 0.5f,
			Top = { Pixels = HeaderMargin / 4f }
		};

		infoRoot.Append(waypointHeader);


	}

	public override void OnActivate()
	{
		base.OnActivate();

		Enabled = true;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();

		Enabled = false;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		UpdateInput();
	}

	private void RootUpdateEvent(UIElement element)
	{
		float target = Enabled ? 0f : Main.screenHeight;

		element.Top.Pixels = MathHelper.SmoothStep(element.Top.Pixels, target, 0.3f);
	}

	private static UIPanel BuildPanel()
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
			Top = { Pixels = HeaderMargin },
			Width = { Pixels = ListFullWidth },
			Height = { Pixels = FullHeight - HeaderMargin - 48f }
		};

		PopulateList(list);

		list.SetScrollbar(new UIScrollbar());

		return list;
	}

	private void PopulateList(UIList list)
	{
		for (int i = 0; i < ModWaypointLoader.WaypointCount; i++)
		{
			ModWaypoint waypoint = ModWaypointLoader.Waypoints[i];

			Asset<Texture2D> icon = ModContent.Request<Texture2D>(waypoint.IconPath, AssetRequestMode.ImmediateLoad);

			var tab = new UIWaypointListElement(icon, waypoint.DisplayName, i);

			tab.OnUpdate += _ => tab.Selected = tab.Index == SelectedWaypointIndex;

			list.Add(tab);

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

		holdDelayTimer++;

		int nextIndex = SelectedWaypointIndex + direction;

		if (nextIndex < 0 || nextIndex > ModWaypointLoader.WaypointCount - 1)
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

		SelectedWaypointIndex = nextIndex;

		thumbnailImage.SetImage(ModContent.Request<Texture2D>(SelectedListWaypoint.PreviewPath));
		waypointHeader.SetText(SelectedListWaypoint.DisplayName);
	}

	private bool CanProcessInput()
	{
		return holdDelayTimer == 0 || holdDelayTimer > KeyInitialDelay && holdDelayTimer % KeyRepeatDelay == 0;
	}
}
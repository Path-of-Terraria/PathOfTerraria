using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.Elements;
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

	private static readonly Asset<Texture2D> PanelBackgroundTexture =
		ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBackground");

	private static readonly Asset<Texture2D> PanelBorderTexture =
		ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBorder");

	public int SelectedWaypointIndex
	{
		get => _selectedWaypointIndex;
		set => _selectedWaypointIndex = (int)MathHelper.Clamp(value, 0, ModWaypointLoader.WaypointCount - 1);
	}

	public UIWaypointListElement SelectedListElement => tabs[SelectedWaypointIndex];
	public ModWaypoint SelectedListWaypoint => ModWaypointLoader.Waypoints[SelectedWaypointIndex];

	private readonly List<UIWaypointListElement> tabs = [];

	private int _selectedWaypointIndex;
	private UIElement buttonElement;

	private UIPanel buttonPanel;
	private UIScalingText buttonText;

	public bool Enabled;

	private int holdDelayTimer;
	private UIElement listRootElement;

	private UIElement rootElement;

	private UIImage thumbnailImage;

	private UIText waypointText;

	public override void OnInitialize()
	{
		base.OnInitialize();

		rootElement = new UIElement
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			Top = { Pixels = Main.screenHeight },
			Width = { Pixels = FullWidth },
			Height = { Pixels = FullHeight }
		};

		rootElement.Append(BuildPanel());

		Append(rootElement);

		listRootElement = new UIElement
		{
			Width = { Pixels = ListFullWidth },
			Height = { Pixels = ListFullHeight },
			PaddingLeft = 8f,
			PaddingRight = 8f
		};

		rootElement.Append(listRootElement);

		var header = new UIText("Waypoints", 1.2f)
		{
			HAlign = 0.5f,
			Top = { Pixels = HeaderMargin / 4f }
		};

		listRootElement.Append(header);

		UIList list = BuildList();

		listRootElement.Append(list);

		buttonElement = new UIElement
		{
			Width = { Pixels = FullWidth },
			Height = { Pixels = 48f },
			VAlign = 1f,
			Top = { Pixels = -8f }
		};

		buttonElement.OnMouseOver += HandleMouseOverSound;
		buttonElement.OnMouseOut += HandleMouseOutSound;

		buttonElement.OnLeftClick += (_, _) =>
		{
			SelectedListWaypoint.Teleport(Main.LocalPlayer);
			Enabled = false;
		};

		listRootElement.Append(buttonElement);

		buttonPanel = new UIPanel(PanelBackgroundTexture, PanelBorderTexture, 13)
		{
			BackgroundColor = new Color(68, 97, 175) * 0.8f,
			BorderColor = new Color(68, 97, 175) * 0.8f,
			OverrideSamplerState = SamplerState.PointClamp,
			Width = { Pixels = FullWidth },
			Height = { Pixels = 48f }
		};

		buttonElement.Append(buttonPanel);

		buttonText = new UIScalingText("Travel")
		{
			HAlign = 0.5f,
			VAlign = 0.5f
		};

		buttonElement.Append(buttonText);

		var separator = new UIImage(TextureAssets.MagicPixel)
		{
			ScaleToFit = true,
			VAlign = 0.5f,
			Left = { Pixels = ListFullWidth + 1f },
			Width = { Pixels = 2f },
			Height = { Pixels = FullHeight - 32f },
			Color = Color.White * 0.5f
		};

		rootElement.Append(separator);

		var infoRoot = new UIElement
		{
			Left = { Pixels = ListFullWidth + ElementPadding },
			Width = { Pixels = InfoFullWidth },
			Height = { Pixels = InfoFullHeight },
			PaddingRight = 8f
		};

		rootElement.Append(infoRoot);

		thumbnailImage = new UIImage(ModContent.Request<Texture2D>(SelectedListWaypoint.PreviewPath, AssetRequestMode.ImmediateLoad))
		{
			ScaleToFit = true,
			Top = { Pixels = HeaderMargin },
			Width = { Pixels = InfoFullWidth },
			Height = { Pixels = InfoFullHeight - HeaderMargin - 8f },
			OverrideSamplerState = SamplerState.PointClamp
		};

		infoRoot.Append(thumbnailImage);

		waypointText = new UIText(SelectedListWaypoint.DisplayName, 1.2f)
		{
			HAlign = 0.5f,
			Top = { Pixels = HeaderMargin / 4f }
		};

		infoRoot.Append(waypointText);

		var closeImage = new UIHoverTooltipImage(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/Close_Icon"), "")
		{
			Left = new StyleDimension(-16, 1),
			ActiveScale = 1.1f,
			Width = StyleDimension.FromPixels(20),
			Height = StyleDimension.FromPixels(20)
		};

		closeImage.OnMouseOut += HandleMouseOutSound;
		closeImage.OnMouseOver += HandleMouseOverSound;
		closeImage.OnLeftClick += (_, _) => Enabled = false;

		infoRoot.Append(closeImage);
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

		if (rootElement.ContainsPoint(Main.MouseScreen))
		{
			Main.LocalPlayer.mouseInterface = true;
		}

		UpdateInput();

		float target = Enabled ? 0f : Main.screenHeight;

		rootElement.Top.Pixels = MathHelper.SmoothStep(rootElement.Top.Pixels, target, 0.3f);

		buttonPanel.BorderColor = Color.Lerp(buttonPanel.BorderColor, buttonElement.IsMouseHovering ? Color.White : new Color(68, 97, 175), 0.3f)
			* 0.8f;

		buttonText.Scale = MathHelper.SmoothStep(buttonText.Scale, buttonElement.IsMouseHovering ? 1.2f : 1f, 0.3f);
	}

	private static UIPanel BuildPanel()
	{
		var panel = new UIPanel(PanelBackgroundTexture, PanelBorderTexture, 13)
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

	private static void HandleMouseOverSound(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = 0.15f,
				MaxInstances = 1
			}
		);
	}

	private static void HandleMouseOutSound(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = -0.25f,
				MaxInstances = 1
			}
		);
	}

	private void PopulateList(UIList list)
	{
		for (int i = 0; i < ModWaypointLoader.WaypointCount; i++)
		{
			ModWaypoint waypoint = ModWaypointLoader.Waypoints[i];

			Asset<Texture2D> icon = ModContent.Request<Texture2D>(waypoint.IconPath, AssetRequestMode.ImmediateLoad);

			var tab = new UIWaypointListElement(icon, waypoint.DisplayName, i);

			tab.OnUpdate += _ => tab.Selected = tab.Index == SelectedWaypointIndex;
			tab.OnLeftClick += (_, _) => SelectedWaypointIndex = tab.Index;

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
		if (!CanProcessInput() || !listRootElement.ContainsPoint(Main.MouseScreen))
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

		thumbnailImage.SetImage(ModContent.Request<Texture2D>(SelectedListWaypoint.PreviewPath, AssetRequestMode.ImmediateLoad));
		waypointText.SetText(SelectedListWaypoint.DisplayName);
	}

	private bool CanProcessInput()
	{
		return holdDelayTimer == 0 || holdDelayTimer > KeyInitialDelay && holdDelayTimer % KeyRepeatDelay == 0;
	}
}
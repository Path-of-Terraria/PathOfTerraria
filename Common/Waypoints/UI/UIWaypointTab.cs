using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointTab : UIElement
{
	/// <summary>
	///     The margin of this element in pixels.
	/// </summary>
	public const float Margin = 16f;

	/// <summary>
	///     Whether this element is selected or not.
	/// </summary>
	public bool Selected { get; set; }

	/// <summary>
	///     The index of this element in a <see cref="UIList" />.
	/// </summary>
	public readonly int Index;

	/// <summary>
	///     The waypoint of this element.
	/// </summary>
	public readonly ModWaypoint? Waypoint;

	private UIPanel panel;

	public UIWaypointTab(ModWaypoint? waypoint, int index)
	{
		Waypoint = waypoint;
		Index = index;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		panel = new UIPanel(
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBackground"),
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBorder"),
			13
		)
		{
			BackgroundColor = new Color(68, 97, 175) * 0.8f,
			BorderColor = new Color(68, 97, 175) * 0.8f,
			OverrideSamplerState = SamplerState.PointClamp
		};

		panel.Width.Set(Width.Pixels, 0f);
		panel.Height.Set(Height.Pixels, 0f);

		Append(panel);

		Asset<Texture2D> texture = ModContent.Request<Texture2D>(Waypoint.IconPath, AssetRequestMode.ImmediateLoad);

		var icon = new UIHoverImage(texture)
		{
			ActiveScale = 1.15f,
			VAlign = 0.5f,
			OverrideSamplerState = SamplerState.PointClamp
		};

		icon.Left.Set(Margin, 0f);

		icon.OnMouseOver += static (_, _) => SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = 0.25f,
				MaxInstances = 1
			}
		);

		icon.OnMouseOut += static (_, _) => SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = -0.25f,
				MaxInstances = 1
			}
		);

		Append(icon);

		var text = new UIText(Waypoint.DisplayName, 0.8f) { VAlign = 0.5f };

		text.Left.Set(icon.Left.Pixels + texture.Width() + (32f - texture.Width()) + Margin, 0f);

		Append(text);

		var separator = new UIImage(TextureAssets.MagicPixel)
		{
			ScaleToFit = true,
			Color = Color.White * 0.8f,
			HAlign = 0.5f,
			VAlign = 1f,
			OverrideSamplerState = SamplerState.PointClamp
		};

		separator.Width.Set(Width.Pixels, 0f);
		separator.Height.Set(2f, 0f);

		Append(separator);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		panel.BorderColor = Color.Lerp(panel.BorderColor, Selected ? Color.White : new Color(68, 97, 175), 0.3f) * 0.8f;
	}
}
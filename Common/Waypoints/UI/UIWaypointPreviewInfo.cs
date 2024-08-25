using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointPreviewInfo : UIElement
{
	/// <summary>
	///     The width of this element in pixels.
	/// </summary>
	public const float FullWidth = 200f;

	/// <summary>
	///     The height of this element in pixels.
	/// </summary>
	public const float FullHeight = UIWaypointBrowser.FullHeight;

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width.Set(FullWidth, 0f);
		Height.Set(FullHeight, 0f);

		Append(BuildPanel());
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
}
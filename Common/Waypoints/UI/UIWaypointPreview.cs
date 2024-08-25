using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointPreview : UIElement
{
	/// <summary>
	///		The width of this element in pixels.
	/// </summary>
	public const float FullWidth = 500f;
	
	/// <summary>
	///		The height of this element in pixels.
	/// </summary>
	public const float FullHeight = UIWaypointBrowser.FullHeight;
	
	public readonly Asset<Texture2D> Thumbnail;
	
	public UIWaypointPreview(Asset<Texture2D> thumbnail)
	{
		Thumbnail = thumbnail;
	}
	
	public override void OnInitialize()
	{
		base.OnInitialize();
		
		Width.Set(FullWidth, 0f);
		Height.Set(FullHeight, 0f);

		Append(BuildPanel());
		Append(BuildThumbnail());
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

	private UIImage BuildThumbnail()
	{
		var image = new UIImage(Thumbnail)
		{
			AllowResizingDimensions = true,
			ScaleToFit = true,
			Color = Color.White * 0.8f,
			HAlign = 0.5f,
			VAlign = 0.5f,
			OverrideSamplerState = SamplerState.PointClamp,
			Width = { Pixels = FullWidth - 50f },
			Height = { Pixels = FullHeight - 100f }
		};

		return image;
	}
}
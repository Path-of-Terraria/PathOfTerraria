using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointPreview : UIElement
{
	/// <summary>
	///     The width of this element in pixels.
	/// </summary>
	public const float FullWidth = 500f;

	/// <summary>
	///     The height of this element in pixels.
	/// </summary>
	public const float FullHeight = UIWaypointBrowser.FullHeight;

	private UIImage image;

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width.Set(FullWidth, 0f);
		Height.Set(FullHeight, 0f);
		
		Append(BuildPanel());
		
		image = BuildThumbnail();
		
		Append(image);
	}

	/// <summary>
	///		Sets the thumbnail texture used in the waypoint preview.
	/// </summary>
	/// <remarks>
	///		This is normally used to update the thumbnail when the selected waypoint changes.
	/// </remarks>
	/// <param name="thumbnail">The new thumbnail texture.</param>
	public void SetThumbnail(Asset<Texture2D> thumbnail)
	{
		image.SetImage(thumbnail);
		
		image.Width.Set(FullWidth - 20f, 0f);
		image.Height.Set(FullHeight - 60f, 0f);
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
		var image = new UIImage(Asset<Texture2D>.Empty)
		{
			AllowResizingDimensions = true,
			ScaleToFit = true,
			Color = Color.White * 0.8f,
			OverrideSamplerState = SamplerState.PointClamp,
			HAlign = 0.5f,
			Top = { Pixels = 50f },
			Width = { Pixels = FullWidth - 50f },
			Height = { Pixels = FullHeight - 150f }
		};

		return image;
	}
}
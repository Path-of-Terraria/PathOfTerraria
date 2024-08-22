using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointPreview : UIElement
{
	/// <summary>
	///		The width of this element in pixels.
	/// </summary>
	public const float FullWidth = 600f;
	
	public readonly Asset<Texture2D> Thumbnail;
	
	public UIWaypointPreview(Asset<Texture2D> thumbnail)
	{
		Thumbnail = thumbnail;
	}
	
	public override void OnInitialize()
	{
		base.OnInitialize();
		
		Width.Set(FullWidth, 0f);
		Height.Set(UIWaypointBrowser.FullHeight, 0f);
		
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
		panel.Height.Set(UIWaypointBrowser.FullHeight, 0f);

		Append(panel);
	}
}
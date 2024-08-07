using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Waypoints;

public sealed class UIWaypointBrowser : UIState
{
	public static readonly Asset<Texture2D> HomeIconTexture = ModContent.Request<Texture2D>(
		$"{PoTMod.ModName}/Assets/UI/Waypoints/Home",
		AssetRequestMode.ImmediateLoad
	);

	public override void OnInitialize()
	{
		base.OnInitialize();
		
		Append(new UIWaypointPreview()
		{
			HAlign = 0.5f,
			VAlign = 0.5f
		});
	}
}
using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Waypoints;

public sealed class UIWaypointMenu : UIState
{
	public static readonly Asset<Texture2D> HomeIconTexture = ModContent.Request<Texture2D>(
		$"{PoTMod.ModName}/Assets/UI/Waypoints/Home",
		AssetRequestMode.ImmediateLoad
	);

	public override void OnInitialize()
	{
		base.OnInitialize();
	}
}
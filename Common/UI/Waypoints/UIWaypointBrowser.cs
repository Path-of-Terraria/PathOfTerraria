using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.Waypoints;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Waypoints;

public sealed class UIWaypointBrowser : UIState
{
	public const string Identifier = $"{PoTMod.ModName}:{nameof(UIWaypointBrowser)}";
	
	public static readonly Asset<Texture2D> HomeIconTexture = ModContent.Request<Texture2D>(
		$"{PoTMod.ModName}/Assets/UI/Waypoints/Home",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> RavencrestIconTexture = ModContent.Request<Texture2D>(
		$"{PoTMod.ModName}/Assets/UI/Waypoints/Ravencrest",
		AssetRequestMode.ImmediateLoad
	);

	public override void OnInitialize()
	{
		base.OnInitialize();

		foreach (ModWaypoint waypoint in ModContent.GetContent<ModWaypoint>())
		{
			Asset<Texture2D> icon = ModContent.Request<Texture2D>(waypoint.IconPath, AssetRequestMode.ImmediateLoad);
		}
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
	}
}
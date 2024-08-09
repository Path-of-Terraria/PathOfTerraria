using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Waypoints;

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

	/// <summary>
	///		The position of the arcane obelisk instance in tile coordinates.
	/// </summary>
	public readonly Point Coordinates;

	public UIWaypointBrowser(Point coordinates)
	{
		Coordinates = coordinates;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
	}
}
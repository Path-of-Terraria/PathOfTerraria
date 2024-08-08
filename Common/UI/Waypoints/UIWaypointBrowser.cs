using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Waypoints;

public sealed class UIWaypointBrowser : UIState
{
	public static readonly Asset<Texture2D> HomeIconTexture = ModContent.Request<Texture2D>(
		$"{PoTMod.ModName}/Assets/UI/Waypoints/Home",
		AssetRequestMode.ImmediateLoad
	);

	public readonly Point Position;

	private UIImage icon;
	
	public UIWaypointBrowser(Point position)
	{
		Position = position;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		icon = new UIImage(HomeIconTexture);
		
		Append(icon);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		icon.Left.Pixels = MathHelper.SmoothStep(icon.Left.Pixels, Position.X * 16f - Main.screenPosition.X, 0.2f);
		icon.Top.Pixels = MathHelper.SmoothStep(icon.Top.Pixels, Position.X * 16f - Main.screenPosition.X, 0.2f);
	}
}
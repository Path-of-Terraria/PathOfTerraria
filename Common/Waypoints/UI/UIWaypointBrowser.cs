using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointBrowser : UIState
{
	/// <summary>
	///     The unique identifier of this state.
	/// </summary>
	public const string Identifier = $"{PoTMod.ModName}:{nameof(UIWaypointBrowser)}";

	/// <summary>
	///     The height of this element in pixels.
	/// </summary>
	public const float FullHeight = 400f;
	
	/// <summary>
	///		Whether this state is enabled or not.
	/// </summary>
	public bool Enabled { get; set; }
	
	public override void OnInitialize()
	{
		base.OnInitialize();

		var root = new UIElement
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = { Pixels = UIWaypointList.FullWidth + UIWaypointPreview.FullWidth + 8f },
			Height = { Pixels = FullHeight }
		};
		
		root.OnUpdate += RootUpdateEvent;
		
		Append(root);

		var list = new UIWaypointList
		{
			VAlign = 0.5f
		};
		
		root.Append(list);

		var preview = new UIWaypointPreview(TextureAssets.Item[ItemID.HermesBoots])
		{
			HAlign = 1f
		};
		
		root.Append(preview);
	}

	private void RootUpdateEvent(UIElement element)
	{
		var target = Enabled ? 0f : Main.screenHeight;
		
		element.Top.Pixels = MathHelper.SmoothStep(element.Top.Pixels, target, 0.3f);
	}

	public override void OnActivate()
	{
		base.OnActivate();

		Enabled = true;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		
		Enabled = false;
	}
}
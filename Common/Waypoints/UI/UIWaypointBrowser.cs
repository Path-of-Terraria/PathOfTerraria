using ReLogic.Content;
using Terraria.GameContent;
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
	///     The width of this state in pixels.
	/// </summary>
	public const float FullWidth = UIWaypointList.FullWidth + UIWaypointPreview.FullWidth + UIWaypointPreviewInfo.FullWidth + ElementPadding * 2f;

	/// <summary>
	///     The height of this state in pixels.
	/// </summary>
	public const float FullHeight = 400f;

	/// <summary>
	///     The padding of each element of this state in pixels.
	/// </summary>
	public const float ElementPadding = 12f;

	/// <summary>
	///     Whether this state is enabled or not.
	/// </summary>
	public bool Enabled { get; set; }

	private UIWaypointList list;

	public override void OnInitialize()
	{
		base.OnInitialize();

		var root = new UIElement
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = { Pixels = FullWidth },
			Height = { Pixels = FullHeight }
		};

		root.OnUpdate += RootUpdateEvent;

		Append(root);

		list = new UIWaypointList { VAlign = 0.5f };

		root.Append(list);

		var preview = new UIWaypointPreview
		{
			VAlign = 0.5f,
			Left = { Pixels = UIWaypointList.FullWidth + ElementPadding }
		};
		
		preview.OnUpdate += PreviewUpdateEvent;

		root.Append(preview);

		var info = new UIWaypointPreviewInfo
		{
			VAlign = 0.5f,
			Left = { Pixels = FullWidth - UIWaypointPreviewInfo.FullWidth }
		};

		root.Append(info);
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
	
	private void RootUpdateEvent(UIElement element)
	{
		float target = Enabled ? 0f : Main.screenHeight;

		element.Top.Pixels = MathHelper.SmoothStep(element.Top.Pixels, target, 0.3f);
	}
	
	private void PreviewUpdateEvent(UIElement element)
	{
		if (element is not UIWaypointPreview preview)
		{
			return;
		}

		ModWaypoint? waypoint = ModWaypointLoader.Waypoints[list.SelectedWaypointIndex];

		if (waypoint == null)
		{
			return;
		}
		
		preview.SetThumbnail(ModContent.Request<Texture2D>(waypoint.PreviewPath, AssetRequestMode.ImmediateLoad));
	}
}
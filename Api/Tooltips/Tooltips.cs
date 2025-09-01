using System.Collections.Generic;
using PathOfTerraria.Common.UI;

namespace PathOfTerraria.Api.Tooltips;

/// <summary> Description used to instantiate a tooltip popup in the <see cref="Tooltip"/> system. </summary>
public struct TooltipDescription()
{
	/// <summary> An identifier used to detect duplicate tooltip instances. </summary>
	public required string Identifier { get; set; }
	/// <summary> If provided, inserts a simple line into the <see cref="Lines"/> list, before <see cref="SimpleSubtitle"/>'s line. </summary>
	public string SimpleTitle { get; set; }
	/// <summary> If provided, inserts a simple line into the <see cref="Lines"/> list, after <see cref="SimpleTitle"/>'s line. </summary>
	public string SimpleSubtitle { get; set; }
	/// <summary> Lines to render. </summary>
	public List<DrawableTooltipLine> Lines { get; set; } = [];
	/// <summary> Tooltip's position. If not provided, will default to that of the cursor. </summary>
	public Vector2? Position { get; set; }
	/// <summary> Tooltip's origin. </summary>
	public Vector2 Origin { get; set; } = Vector2.Zero;
	/// <summary> The higher this value is, the less likely this tooltip is to move when colliding with other tooltips. </summary>
	public int Stability { get; set; }
	/// <summary> If provided, this item instance is used to invoke ItemLoader's Pre/PostDrawTooltip(Line) hooks. </summary>
	public Item AssociatedItem { get; set; }
	/// <summary> The amount of time in game ticks that this tooltip should be visible for.  </summary>
	public uint VisibilityTimeInTicks { get; set; } = 1;
	/// <summary> Padding to add inside the tooltip's background. </summary>
	public Vector2 Padding { get; set; } = new(16, 16);
}

/// <summary> Draws any amount of popup tooltips when various elements of the UI are hovered over. </summary>
public static class Tooltips
{
	/// <summary> Enqueues a new tooltip to be drawn in the usual interface layer. </summary>
	public static void Create(in TooltipDescription args)
	{
		TooltipsImpl.Create(args);
	}
}

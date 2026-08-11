using System.Collections.Generic;
using PathOfTerraria.Core.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Components;

/// <summary> When added to an element, preserves various data, such as dimensions and scrollbar positions, in a global hashmap, accessed by a string identifier. </summary>
internal sealed class UIPersistent(string identifier, bool preservePosition = true, bool preserveSize = true) : UIComponent
{
	private struct Data
	{
		public required (StyleDimension Left, StyleDimension Top, StyleDimension Width, StyleDimension Height) Dimensions;
		public float ScrollbarPosition;
	}

	private static readonly Dictionary<string, Data> cache = [];

	public string Identifier { get; } = identifier;

	private bool performedImportInUpdate;

	protected override void OnAttach(UIElement element)
	{
		element.OnUpdate += OnUpdate;

		Import(element);
	}

	protected override void OnDetach(UIElement element)
	{
		element.OnUpdate -= OnUpdate;
	}

	public void Import(UIElement element)
	{
		if (cache.TryGetValue(Identifier, out Data data))
		{
			if (preservePosition)
			{
				(element.Left, element.Top) = (data.Dimensions.Left, data.Dimensions.Top);
			}

			if (preserveSize)
			{
				(element.Width, element.Height) = (data.Dimensions.Width, data.Dimensions.Height);
			}

			if (element is UIScrollbar scrollbar)
			{
				scrollbar.ViewPosition = data.ScrollbarPosition;
			}
		}
	}

	private void OnUpdate(UIElement element)
	{
		// Scrollbars use clamps that might not have yet been adequate in OnAttach.
		if (!performedImportInUpdate)
		{
			if (element is UIScrollbar)
			{
				Import(element);
			}

			performedImportInUpdate = true;
		}

		cache[Identifier] = new Data
		{
			Dimensions = (element.Left, element.Top, element.Width, element.Height),
			ScrollbarPosition = element is UIScrollbar scrollbar ? scrollbar.ViewPosition : 0f
		};
	}
}

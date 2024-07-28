using System.Collections.Generic;

namespace PathOfTerraria.Common.UI.Hotbar.Components.Sets;

internal sealed class BuildingSet : HotbarSet
{
	private sealed class BuildingSpecialSlot : SpecialSlot
	{
	}

	public override IEnumerable<HotbarSlot> HotbarSlots => throw new NotImplementedException();
}

using System.Collections.Generic;

namespace PathOfTerraria.Common.UI.Hotbar.Components.Sets;

internal sealed class CombatSet : HotbarSet
{
	private sealed class CombatSpecialSlot : SpecialSlot
	{
	}

	public override IEnumerable<HotbarSlot> HotbarSlots => throw new NotImplementedException();
}

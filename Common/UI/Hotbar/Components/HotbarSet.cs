using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Common.UI.Hotbar.Components;

internal abstract class HotbarSet
{
	public SpecialSlot SpecialSlot { get; }

	public abstract IEnumerable<HotbarSlot> HotbarSlots { get; }

	public virtual float GetFullWidth()
	{
		return HotbarSlots.Sum(x => x.GetFullWidth()); // TODO
	}
}

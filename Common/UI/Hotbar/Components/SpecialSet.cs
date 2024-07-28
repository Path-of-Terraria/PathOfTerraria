using System.Linq;

namespace PathOfTerraria.Common.UI.Hotbar.Components;

internal sealed class SpecialSet
{
	public float PaddingBetweenSlots { get; set; } = 2f;

	private readonly HotbarUI hotbar;

	public SpecialSet(HotbarUI hotbar)
	{
		this.hotbar = hotbar;
	}

	public void Draw(SpriteBatch sb)
	{

	}

	public float GetFullWidth()
	{
		return (hotbar.Sets.Count - 1) * PaddingBetweenSlots + hotbar.Sets.Sum(x => x.SpecialSlot.GetFullWidth());
	}
}

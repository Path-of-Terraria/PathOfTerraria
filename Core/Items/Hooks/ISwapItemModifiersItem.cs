using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface ISwapItemModifiersItem
{
	void SwapItemModifiers(Item item, EntityModifier swapItemModifier);

	public static void Invoke(Item item, EntityModifier swapItemModifier)
	{
		if (item.TryGetInterface(out ISwapItemModifiersItem instance))
		{
			instance.SwapItemModifiers(item, swapItemModifier);
		}
	}
}

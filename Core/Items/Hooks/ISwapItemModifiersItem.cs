using PathOfTerraria.Core.Systems;

namespace PathOfTerraria.Core.Items.Hooks;

public interface ISwapItemModifiersItem
{
	void SwapItemModifiers(Item item, EntityModifier SwapItemModifier);
}

using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IItemLevelControllerItem
{
	int ItemLevel { get; set; }

	public static int GetLevel(Item item)
	{
		if (item.TryGetInterface(out IItemLevelControllerItem itemLevelControllerItem))
		{
			return itemLevelControllerItem.ItemLevel;
		}

		return item.GetInstanceData().RealLevel;
	}

	public static void SetLevel(Item item, int level)
	{
		if (item.TryGetInterface(out IItemLevelControllerItem itemLevelControllerItem))
		{
			itemLevelControllerItem.ItemLevel = level;
		}
		else
		{
			item.GetInstanceData().RealLevel = level;
		}
	}
}

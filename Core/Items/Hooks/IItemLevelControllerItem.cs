using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IItemLevelControllerItem
{
	int ItemLevel { get; set; }

	public static int GetLevel(Item item)
	{
		if (item.TryGetInterfaces(out IItemLevelControllerItem[] itemLevelControllerItem))
		{
			if (itemLevelControllerItem.Length != 1)
			{
				throw new Exception("Cannot have more than one IItemLevelControllerItem interface on a single item");
			}

			return itemLevelControllerItem[0].ItemLevel;
		}

		return item.GetInstanceData().RealLevel;
	}

	public static void SetLevel(Item item, int level)
	{
		if (item.TryGetInterfaces(out IItemLevelControllerItem[] itemLevelControllerItem))
		{
			if (itemLevelControllerItem.Length != 1)
			{
				throw new Exception("Cannot have more than one IItemLevelControllerItem interface on a single item");
			}

			itemLevelControllerItem[0].ItemLevel = level;
		}
		else
		{
			item.GetInstanceData().RealLevel = level;
		}
	}
}

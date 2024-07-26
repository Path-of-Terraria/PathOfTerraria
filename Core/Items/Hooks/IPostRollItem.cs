using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IPostRollItem
{
	void PostRoll(Item item);

	public static void Invoke(Item item)
	{
		if (item.TryGetInterfaces(out IPostRollItem[] instances))
		{
			foreach (IPostRollItem instance in instances)
			{
				instance.PostRoll(item);
			}
		}
	}
}
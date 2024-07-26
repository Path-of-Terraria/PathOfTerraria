using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IExtraRollsItem
{
	void ExtraRolls(Item item);

	public static void Invoke(Item item)
	{
		if (item.TryGetInterface(out IExtraRollsItem instance))
		{
			instance.ExtraRolls(item);
		}
	}
}

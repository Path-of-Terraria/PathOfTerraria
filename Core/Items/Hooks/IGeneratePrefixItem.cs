using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IGeneratePrefixItem
{
	string GeneratePrefix(Item item);

	public static string Invoke(Item item)
	{
		if (item.TryGetInterface(out IGeneratePrefixItem generatePrefixItem))
		{
			return generatePrefixItem.GeneratePrefix(item);
		}

		return "";
	}
}

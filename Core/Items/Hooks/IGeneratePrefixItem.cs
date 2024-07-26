using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IGeneratePrefixItem
{
	string GeneratePrefix(Item item);

	public static string Invoke(Item item)
	{
		if (item.TryGetInterfaces(out IGeneratePrefixItem[] generatePrefixItem))
		{
			if (generatePrefixItem.Length != 1)
			{
				throw new Exception("Cannot have more than one IGeneratePrefixItem interface on a single item");
			}

			return generatePrefixItem[0].GeneratePrefix(item);
		}

		return "";
	}
}

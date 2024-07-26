using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IGenerateSuffixItem
{
	string GenerateSuffix(Item item);

	public static string Invoke(Item item)
	{
		if (item.TryGetInterfaces(out IGenerateSuffixItem[] generateSuffixItem))
		{
			if (generateSuffixItem.Length != 1)
			{
				throw new Exception("Cannot have more than one IGenerateSuffixItem interface on a single item");
			}

			return generateSuffixItem[0].GenerateSuffix(item);
		}

		return "";
	}
}

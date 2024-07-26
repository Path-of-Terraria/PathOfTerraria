using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IGenerateSuffixItem
{
	string GenerateSuffix(Item item);

	public static string Invoke(Item item)
	{
		if (item.TryGetInterface(out IGenerateSuffixItem generateSuffixItem))
		{
			return generateSuffixItem.GenerateSuffix(item);
		}

		return "";
	}
}

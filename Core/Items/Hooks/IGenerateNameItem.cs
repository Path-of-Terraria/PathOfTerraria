using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IGenerateNameItem
{
	string GenerateName(Item item);

	public static string Invoke(Item item)
	{
		if (item.TryGetInterface(out IGenerateNameItem generateNameItem))
		{
			return generateNameItem.GenerateName(item);
		}

		return item.GetInstanceData().Rarity switch
		{
			Rarity.Normal => item.Name,
			Rarity.Magic => $"{IGeneratePrefixItem.Invoke(item)} {item.Name}",
			Rarity.Rare => $"{IGeneratePrefixItem.Invoke(item)} {item.Name} {IGenerateSuffixItem.Invoke(item)}",
			Rarity.Unique => item.Name, // uniques might just want to override the GenerateName function
			_ => "Unknown Item"
		};
	}
}

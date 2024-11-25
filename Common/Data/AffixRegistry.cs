using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.Data;

public class AffixRegistry : ILoadable
{
	/// <summary>
	/// A map of AffixData objects, with the key being the type from NPC.
	/// </summary>
	private static Dictionary<Type, ItemAffixData> ItemAffixData = [];

	public void Load(Mod mod)
	{
		ItemAffixData = LoadJsonFilesToMapAsync();

		foreach (KeyValuePair<Type, ItemAffixData> entry in ItemAffixData)
		{
			Console.WriteLine($"Affix with type key: \"{entry.Key}\" registered.");
		}
	}

	public virtual void Unload() { }

	/// <summary>
	/// Provides a safe way of getting ItemAffixData from the ItemAffixData map.
	/// </summary>
	/// <param name="type">Type to look for.</param>
	/// <returns><see cref="Models.ItemAffixData"/> instance of the given type.</returns>
	public static ItemAffixData TryGetAffixData(Type type)
	{
		try
		{
			return ItemAffixData[type];
		}
		catch (KeyNotFoundException exception)
		{
			Console.WriteLine($"ItemAffixData with type {type.Name} not found.\n\n{exception}");
			return null;
		}
	}

	/// <summary>
	/// Provides a safe way of getting ItemAffixData from the ItemAffixData map.
	/// </summary>
	/// <typeparam name="T">Type to look for.</typeparam>
	/// <returns><see cref="Models.ItemAffixData"/> instance of the given type.</returns>
	public static ItemAffixData TryGetAffixData<T>() where T : Affix
	{
		return TryGetAffixData(typeof(T));
	}

	/// <summary>
	/// Loads the JSON files from the paths.txt file and returns a map of the data.
	/// </summary>
	/// <returns></returns>
	private static Dictionary<Type, ItemAffixData> LoadJsonFilesToMapAsync()
	{
		var jsonDataMap = new Dictionary<Type, ItemAffixData>();
		var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

		List<string> jsonFiles = PoTMod.Instance.GetFileNames();
		IEnumerable<Type> types = AssemblyManager.GetLoadableTypes(ModContent.GetInstance<PoTMod>().Code)
			.Where(x => typeof(Affix).IsAssignableFrom(x) && !x.IsAbstract);

		foreach (Stream jsonStream in from path in jsonFiles
		         where path.StartsWith("Common/Data/Affixes") && path.EndsWith(".json")
		         select PoTMod.Instance.GetFileStream(path))
		{
			using var jsonReader = new StreamReader(jsonStream);
			string json = jsonReader.ReadToEnd();
			List<ItemAffixData> datas = JsonSerializer.Deserialize<List<ItemAffixData>>(json, options);

			if (datas == null)
			{
				continue;
			}

			foreach (ItemAffixData data in datas)
			{
				Type type = types.FirstOrDefault(x => x.Name == data.AffixType);

				if (type is null || !jsonDataMap.TryAdd(type, data))
				{
					Console.WriteLine($"Affix of type {data.AffixType} not found.");
				}
			}
		}

		return jsonDataMap;
	}
	
	/// <summary>
	/// Convert ItemAffixData to ItemAffix instance based on affix type.
	/// </summary>
	/// <param name="affixData">ItemAffixData containing affix details.</param>
	/// <returns>Instance of ItemAffix corresponding to the affix data.</returns>
	internal static ItemAffix ConvertToItemAffix(ItemAffixData affixData)
	{
		string typeName = $"{PoTMod.ModName}.Common.Systems.Affixes.ItemTypes.{affixData.AffixType}";
		var affixType = Type.GetType(typeName);

		if (affixType == null || !typeof(ItemAffix).IsAssignableFrom(affixType))
		{
			return null; // Handle case where affix type isn't found or isn't a valid ItemAffix
		}

		var affixInstance = (ItemAffix)Activator.CreateInstance(affixType);

		return affixInstance;
	}

	
	/// <summary>
	/// Filters ItemAffixData dictionary by ItemType and selects a random affix.
	/// </summary>
	/// <param name="itemType">The ItemType to filter by.</param>
	/// <returns>Random ItemAffixData entry matching the ItemType.</returns>
	public static ItemAffixData GetRandomAffixDataByItemType(ItemType itemType)
	{
		var filteredAffixData = ItemAffixData.Values
			.Where(affixData => (itemType & affixData.GetEquipTypes()) != ItemType.None)
			.ToList();

		if (filteredAffixData.Count == 0)
		{
			return null; // No matching affix found
		}

		int randomIndex = Main.rand.Next(0, filteredAffixData.Count);

		return filteredAffixData[randomIndex];
	}

	
	/// <summary>
	/// Retrieves a random affix value for the given ItemAffix.
	/// </summary>
	/// <param name="affix">The ItemAffix instance.</param>
	/// <param name="itemLevel">The level of the item for determining appropriate tier.</param>
	/// <returns>Random affix value.</returns>
	internal static float GetRandomAffixValue(ItemAffix affix, int itemLevel)
	{
		// Get the corresponding ItemAffixData for the affix's type
		Type affixType = affix.GetType();
		if (!ItemAffixData.TryGetValue(affixType, out ItemAffixData affixData))
		{
			throw new ArgumentException($"ItemAffixData not found for affix type: {affixType.Name}");
		}

		if (itemLevel == 0)
		{
			itemLevel = 1;
		}
		
		// Get the appropriate TierData based on itemLevel
		ItemAffixData.TierData tierData = affixData.GetAppropriateTierData(itemLevel);

		// Generate a random value within the specified range
		float randomValue = GenerateRandomValue(tierData.MinValue, tierData.MaxValue);

		return randomValue;
	}

	/// <summary>
	/// Generates a random value within the specified range.
	/// </summary>
	/// <param name="min">Minimum value (inclusive).</param>
	/// <param name="max">Maximum value (inclusive).</param>
	/// <returns>Random value within the range.</returns>
	private static float GenerateRandomValue(float min, float max)
	{
		return (float)(Main.rand.NextDouble() * (max - min) + min);
	}
}
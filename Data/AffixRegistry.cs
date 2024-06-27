using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using PathOfTerraria.Data.Models;
using PathOfTerraria.Core.Systems.Affixes;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Data;

public class AffixRegistry : ILoadable
{
	/// <summary>
	/// A map of AffixData objects, with the key being the type from NPC.
	/// </summary>
	private static Dictionary<Type, ItemAffixData> _itemAffixData = [];
	
	public void Load(Mod mod)
	{
		_itemAffixData = LoadJsonFilesToMapAsync();

		foreach (KeyValuePair<Type, ItemAffixData> entry in _itemAffixData)
		{
			Console.WriteLine($"Affix with type key: \"{entry.Key}\" registered.");
		}
	}
	
	public virtual void Unload() { }

	/// <summary>
	/// Provides a safe way of getting ItemAffixData from the ItemAffixData map.
	/// </summary>
	/// <param name="type">Type to look for.</param>
	/// <returns><see cref="ItemAffixData"/> instance of the given type.</returns>
	public static ItemAffixData TryGetAffixData(Type type)
	{
		try
		{
			return _itemAffixData[type];
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
	/// <returns><see cref="ItemAffixData"/> instance of the given type.</returns>
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
		var options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		List<string> jsonFiles = PathOfTerraria.Instance.GetFileNames();
		IEnumerable<Type> types = AssemblyManager.GetLoadableTypes(ModContent.GetInstance<PathOfTerraria>().Code).Where(x => typeof(Affix).IsAssignableFrom(x) && !x.IsAbstract);

		foreach (Stream jsonStream in from path in jsonFiles where path.StartsWith("Data/Affixes") && path.EndsWith(".json") select PathOfTerraria.Instance.GetFileStream(path))
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
	/// Using the weight system, grabs an entry from the mob data provided the mobId
	/// </summary>
	/// <param name="mobId"></param>
	/// <returns></returns>
	//public static MobEntry SelectMobEntry(int mobId)
	//{
	//	if (!_itemAffixData.TryGetValue(mobId, out MobData mobData))
	//	{
	//		return null;
	//	}

	//	List<MobEntry> entries = mobData.Entries;
	//	if (entries == null || entries.Count == 0)
	//	{
	//		return null;
	//	}

	//	// Calculate total weight
	//	decimal totalWeight = entries.Sum(e => e.Weight);

	//	// Generate a random number between 0 and total weight
	//	var random = new Random();
	//	decimal randomWeight = (decimal)random.NextDouble() * totalWeight;

	//	// Select the entry based on the random number
	//	decimal cumulativeWeight = 0;
	//	foreach (MobEntry entry in entries)
	//	{
	//		cumulativeWeight += entry.Weight;
	//		if (randomWeight <= cumulativeWeight)
	//		{
	//			return entry;
	//		}
	//	}

	//	// Fallback, should not reach here
	//	return entries.Last();
	//}
}
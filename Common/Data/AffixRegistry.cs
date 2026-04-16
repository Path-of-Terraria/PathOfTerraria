using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Utilities;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.Data;

public class AffixRegistry : ILoadable
{
	/// <summary> All item affix data objects. </summary>
	private static readonly List<ItemAffixData> AllItemData = [];
	/// <summary> Lookup by affix type. </summary>
	private static readonly Dictionary<Type, List<ItemAffixData>> ByAffix = [];
	/// <summary> Lookup by EXACT item type. </summary>
	private static readonly Dictionary<ItemType, List<ItemAffixData>> ByItemType = [];
	/// <summary> Lookup by affix and EXACT item type. </summary>
	private static readonly Dictionary<(Type AffixType, ItemType ItemType), ItemAffixData> ByAffixAndItemType = [];

	public void Load(Mod mod)
	{
		LoadJsonFilesToMap();

		foreach (string entry in AllItemData.Select(d => $"'{d.EquipTypes}'->'{d.AffixType}'").OrderBy(x => x))
		{
#if DEBUG
			mod.Logger.Debug($"Affix data registered: {entry}.");
#endif
		}
	}

	public virtual void Unload() { }

#nullable enable

	/// <summary> Returns one or more affix item distributions entry specific to the provided arguments. Errors if none are found! </summary>
	public static IEnumerable<ItemAffixData> GetItemData(Type affixType)
	{
		if (ByAffix.TryGetValue(affixType, out List<ItemAffixData>? values) && values.Count != 0)
		{
			return values;
		}

		throw new KeyNotFoundException($"No item data found for affix type '{affixType.Name}'!");
	}
	/// <summary> Unsafely acquires an ItemAffixData entry specific to the provided arguments. Errors if no such data exists! </summary>
	public static ItemAffixData GetItemData(Type affixType, Item item)
	{
		return GetItemData(affixType, item.ResolveToSingleType(item.GetInstanceData().ItemType));
	}
	/// <summary> Unsafely acquires an ItemAffixData entry specific to the provided arguments. Errors if no such data exists! </summary>
	public static ItemAffixData GetItemData(Type affixType, ItemType itemType)
	{
		if (TryGetItemData(affixType, itemType) is { } values)
		{
			return values;
		}

		throw new KeyNotFoundException($"No item data found for affix-type pair ('{affixType.Name}', '{itemType}')!");
	}

	/// <summary> Returns zero or more affix item distributions entry specific to the provided arguments. </summary>
	public static IEnumerable<ItemAffixData>? TryGetItemData(Type affixType)
	{
		return ByAffix.TryGetValue(affixType, out List<ItemAffixData>? values) ? values : null;
	}
	/// <summary> Safely tries to acquire an affix item distribution entry specific to the provided arguments. </summary>
	public static ItemAffixData? TryGetItemData(Type affixType, Item item)
	{
		return TryGetItemData(affixType, item.ResolveToSingleType(item.GetInstanceData().ItemType));
	}
	/// <summary> Safely tries to acquire an affix item distribution entry specific to the provided arguments. </summary>
	public static ItemAffixData? TryGetItemData(Type affixType, ItemType itemType)
	{
		if (itemType == 0) { return null; }
		//Debug.Assert(BitOperations.PopCount((ulong)itemType) == 1);
		return ByAffixAndItemType.TryGetValue((affixType, itemType), out ItemAffixData? result) ? result : null;
	}

	/// <summary>
	/// Loads the JSON files from the paths.txt file and returns a map of the data.
	/// </summary>
	private static void LoadJsonFilesToMap()
	{
		AllItemData.Clear();

		var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

		List<string> jsonFiles = PoTMod.Instance.GetFileNames();
		var typesByName = AssemblyManager.GetLoadableTypes(ModContent.GetInstance<PoTMod>().Code)
			.Where(x => typeof(Affix).IsAssignableFrom(x) && !x.IsAbstract)
			.ToDictionary(x => x.Name);

		ByAffixAndItemType.EnsureCapacity(typesByName.Count);
		ByItemType.EnsureCapacity((int)ItemType.TypeCount);

		foreach (Stream jsonStream in jsonFiles
			.Where(path => path.StartsWith("Common/Data/Affixes") && path.EndsWith(".json"))
			.Select(path => PoTMod.Instance.GetFileStream(path)))
		{
			using var jsonReader = new StreamReader(jsonStream);
			string json = jsonReader.ReadToEnd();
			ItemAffixData[] datas = JsonSerializer.Deserialize<ItemAffixData[]>(json, options)!;

			if (datas is not { Length: > 0 }) { continue; }

			AllItemData.EnsureCapacity(AllItemData.Count + datas.Length);

			foreach (ItemAffixData data in datas)
			{
				if (!typesByName.TryGetValue(data.AffixType, out Type? affixType))
				{
					PoTMod.Instance.Logger.Warn($"Affix of type {data.AffixType} not found.");
					continue;
				}

				AddAffixData(affixType, data);
			}
		}
	}

#nullable disable

	private static void AddAffixData(Type affixType, ItemAffixData data)
	{
		static void AddToDictList<K, V>(Dictionary<K, List<V>> dict, K key, V value)
		{
			if (!dict.TryGetValue(key, out List<V> list)) { dict[key] = [value]; }
			else { list.Add(value); }
		}

		AllItemData.Add(data);
		AddToDictList(ByAffix, affixType, data);

		ItemType types = data.GetEquipTypes();
		BitMask<ulong> bits = new((ulong)types);

		foreach (int bitIndex in bits)
		{
			var itemType = (ItemType)(1ul << bitIndex);

			AddToDictList(ByItemType, itemType, data);

			if (!ByAffixAndItemType.TryAdd((affixType, itemType), data))
			{
				throw new InvalidDataException($"Duplicate item affix data found for key ({affixType.Name}, {itemType})!");
			}
		}
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

#nullable enable

	/// <summary>
	/// Filters ItemAffixData dictionary by ItemType and selects a random affix.
	/// </summary>
	/// <param name="itemType">The ItemType to filter by.</param>
	/// <returns>Random ItemAffixData entry matching the ItemType.</returns>
	public static ItemAffixData? GetRandomAffixDataByItemType(ItemType itemType, IEnumerable<ItemAffixData>? excludedAffixes = null)
	{
		if (itemType == 0) 
		{
			throw new ArgumentNullException(nameof(itemType));
		}

		IEnumerable<ItemAffixData> enumerable = AllItemData
			.Where(affixData => (itemType & affixData.GetEquipTypes()) != ItemType.None);

		if (excludedAffixes != null)
		{
			enumerable = enumerable.Except(excludedAffixes);
		}

		var filteredAffixData = enumerable.ToList();

		if (filteredAffixData.Count == 0)
		{
			return null; // No matching affix found
		}

		int randomIndex = Main.rand.Next(0, filteredAffixData.Count);

		return filteredAffixData[randomIndex];
	}

	public static ItemAffixData? GetRandomAffixData(Item item, IEnumerable<ItemAffixData>? excludedAffixes = null)
	{
		ItemType itemType = item.ResolveToSingleType(item.GetInstanceData().ItemType);
		return GetRandomAffixDataByItemType(itemType, excludedAffixes);
	}

	/// <summary>
	/// Retrieves a random affix value for the given ItemAffix.
	/// </summary>
	/// <param name="affix">The ItemAffix instance.</param>
	/// <param name="itemLevel">The level of the item for determining appropriate tier.</param>
	/// <returns>Random affix value.</returns>
	internal static float GetRandomAffixValue(ItemAffix affix, Item item, int itemLevel)
	{
		// Get the corresponding ItemAffixData for the affix's type
		Type affixType = affix.GetType();
		ItemAffixData affixData = GetItemData(affixType, item);

		if (itemLevel == 0)
		{
			itemLevel = 1;
		}

		// Get the appropriate TierData based on itemLevel
		ItemAffixData.TierData tierData = affixData.GetAppropriateTierData(itemLevel, out int tier);
		affix.Tier = tier;

		if (tierData is null)
		{
			return 0;
		}

		// Generate a random value within the specified range
		float randomValue = Main.rand.NextFloat(tierData.MinValue, tierData.MaxValue);

		if (affix is MapAffix map)
		{
			map.Strength = tierData.Strength;
		}

		return affix.Round ? (float)Math.Round(randomValue) : randomValue;
	}
}

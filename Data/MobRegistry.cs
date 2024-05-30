﻿using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using PathOfTerraria.Data.Models;

namespace PathOfTerraria.Data;

public class MobRegistry
{
	/// <summary>
	/// A map of MobData objects, with the key being the type from NPC.
	/// </summary>
	private static Dictionary<int, MobData> _mobData = new();

	public static void Load()
	{
		_mobData = LoadJsonFilesToMapAsync();
		foreach (KeyValuePair<int, MobData> entry in _mobData.Where(entry => entry.Value.Entries.Count > 0))
		{
			Console.WriteLine($"Mob With Key: {entry.Key} - Registered");
		}
	}
	
	/// <summary>
	/// Provides a safe way of getting MobData from the MobData map.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static MobData TryGetMobData(int id)
	{
		try
		{
			return _mobData[id];
		} catch (KeyNotFoundException)
		{
			Console.WriteLine($"MobData with path {id} not found");
			return null;
		}
	}
	
	/// <summary>
	/// Loads the JSON files from the paths.txt file and returns a map of the data.
	/// </summary>
	/// <returns></returns>
	private static Dictionary<int, MobData> LoadJsonFilesToMapAsync()
	{
		var jsonDataMap = new Dictionary<int, MobData>();
		var options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
			
		Stream pathsStream = PathOfTerraria.Instance.GetFileStream("Data/paths.txt");
		var pathsReader = new StreamReader(pathsStream);
		string[] paths = pathsReader.ReadToEnd().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

		pathsReader.Dispose();
		pathsStream.Dispose();
			
		foreach (string path in paths)
		{
			Stream jsonStream = PathOfTerraria.Instance.GetFileStream("Data/" + path);
			var jsonReader = new StreamReader(jsonStream);
			string json = jsonReader.ReadToEnd();
			MobData data = JsonSerializer.Deserialize<MobData>(json, options);
			if (data != null)
			{
				if (Enum.IsDefined(typeof(MobNetIdEnum), data.NetId))
				{
					int enumValue = data.NetId;
					jsonDataMap.Add(enumValue, data);
				}
				else
				{
					Console.WriteLine("Invalid enum value");
				}
			}

			jsonReader.Dispose();
			jsonStream.Dispose();
		}

		return jsonDataMap;
	}
	
	/// <summary>
	/// Using the weight system, grabs an entry from the mob data provided the mobId
	/// </summary>
	/// <param name="mobId"></param>
	/// <returns></returns>
	public static MobEntry SelectMobEntry(int mobId)
	{
		if (!_mobData.TryGetValue(mobId, out MobData mobData))
		{
			return null;
		}

		List<MobEntry> entries = mobData.Entries;
		if (entries == null || entries.Count == 0)
		{
			return null;
		}

		// Calculate total weight
		decimal totalWeight = entries.Sum(e => e.Weight);

		// Generate a random number between 0 and total weight
		var random = new Random();
		decimal randomWeight = (decimal)random.NextDouble() * totalWeight;

		// Select the entry based on the random number
		decimal cumulativeWeight = 0;
		foreach (MobEntry entry in entries)
		{
			cumulativeWeight += entry.Weight;
			if (randomWeight <= cumulativeWeight)
			{
				return entry;
			}
		}

		// Fallback, should not reach here
		return entries.Last();
	}
}
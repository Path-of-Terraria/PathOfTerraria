using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PathOfTerraria.Data.Models;

namespace PathOfTerraria.Data;

public class MobRegistry
{
	public static Dictionary<string, MobData> JsonDataMap = new();
	public static void Load()
	{
		JsonDataMap = LoadJsonFilesToMapAsync();
		// Example of using the map
		foreach (KeyValuePair<string, MobData> entry in JsonDataMap)
		{
			if (entry.Value.Entries.Count > 0)
			{
				Console.WriteLine($"Key: {entry.Key}, Weight: {entry.Value.Entries[0].Weight}");	
			}
		}
	}


	private static Dictionary<string, MobData> LoadJsonFilesToMapAsync()
	{
		var jsonDataMap = new Dictionary<string, MobData>();
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
				jsonDataMap[path] = data;
			}
			jsonReader.Dispose();
			jsonStream.Dispose();
		}

		return jsonDataMap;
	}
}
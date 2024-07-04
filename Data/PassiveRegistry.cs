using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PathOfTerraria.Core;
using PathOfTerraria.Data.Models;

namespace PathOfTerraria.Data;

public class PassiveRegistry : ILoadable
{
	/// <summary>
	/// A map of MobData objects, with the key being the type from NPC.
	/// </summary>
	private static readonly List<PassiveData> Passives = new();
	
	public void Load(Mod mod)
	{
		LoadJsonFilesToMapAsync();
	}
	
	public virtual void Unload() { }

	/// <summary>
	/// Provides a safe way of getting MobData from the MobData map.
	/// </summary>
	/// <param name="playerClass">The playerClass for the passive you want to fetch</param>
	/// <returns></returns>
	public static List<PassiveData> GetPassiveData()
	{
		return Passives;
	}
	
	/// <summary>
	/// Loads the JSON files from the paths.txt file and returns a map of the data.
	/// </summary>
	/// <returns></returns>
	private static void LoadJsonFilesToMapAsync()
	{
		var options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		Stream passiveStream = PathOfTerraria.Instance.GetFileStream($"Data/Passives.json");
		using var passiveReader = new StreamReader(passiveStream);
		string passiveJson = passiveReader.ReadToEnd();
		List<PassiveData> passiveData = JsonSerializer.Deserialize<List<PassiveData>>(passiveJson, options);

		passiveData // no clue how to handle empty values, lol
			.ForEach(d => {
				d.Connections ??= [];
				d.Position ??= new PassivePosition();
			});

		Passives.AddRange(passiveData);
		passiveStream.Close();
		passiveReader.Close();
	}
}
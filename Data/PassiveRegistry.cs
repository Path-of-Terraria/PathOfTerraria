using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Data.Models;

namespace PathOfTerraria.Data;

public class PassiveRegistry : ILoadable
{
	/// <summary>
	/// A map of MobData objects, with the key being the type from NPC.
	/// </summary>
	private static Dictionary<PlayerClass, List<PassiveData>> _passives = new();
	
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
	public static List<PassiveData> TryGetPassiveData(PlayerClass playerClass)
	{
		try
		{
			return _passives[playerClass];
		} catch (KeyNotFoundException)
		{
			Console.WriteLine($"Passive with class {playerClass} not found");
			return [];
		}
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

		foreach (Tuple<PlayerClass, string> tree in new List<Tuple<PlayerClass, string>> {
			new(PlayerClass.Melee, "Melee"),
			new(PlayerClass.Ranged, "Ranged"),
			new(PlayerClass.Magic, "Magic"),
			new(PlayerClass.Summoner, "Summoner")})
		{
			Stream passiveStream = PathOfTerraria.Instance.GetFileStream($"Data/Passives/{tree.Item2}Passives.json");
			using var passiveReader = new StreamReader(passiveStream);
			string passiveJson = passiveReader.ReadToEnd();
			List<PassiveData> passiveData = JsonSerializer.Deserialize<List<PassiveData>>(passiveJson, options);

			passiveData // no clue how to handle empty values, lol
				.ForEach(d => {
					d.Connections ??= [];
					d.Position ??= new PassivePosition();
				});

			_passives.Add(tree.Item1, passiveData);
			passiveStream.Close();
			passiveReader.Close();
		}
	}
}
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
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
			return null;
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
		
		//Melee
		Stream meleeStream = PathOfTerraria.Instance.GetFileStream("Data/Passives/MeleePassives.json");
		using var meleeReader = new StreamReader(meleeStream);
		string meleeJson = meleeReader.ReadToEnd();
		List<PassiveData> meleeData = JsonSerializer.Deserialize<List<PassiveData>>(meleeJson, options);
		_passives.Add(PlayerClass.Melee, meleeData);
		meleeStream.Close();
		meleeReader.Close();
		
		//Ranged
		Stream rangedStream = PathOfTerraria.Instance.GetFileStream("Data/Passives/RangedPassives.json");
		using var rangedReader = new StreamReader(rangedStream);
		string rangedJson = rangedReader.ReadToEnd();
		List<PassiveData> rangedData = JsonSerializer.Deserialize<List<PassiveData>>(rangedJson, options);
		_passives.Add(PlayerClass.Ranged, rangedData);
		rangedStream.Close();
		rangedReader.Close();
		
		//Magic
		Stream magicStream = PathOfTerraria.Instance.GetFileStream("Data/Passives/MagicPassives.json");
		using var magicReader = new StreamReader(magicStream);
		string magicJson = magicReader.ReadToEnd();
		List<PassiveData> magicData = JsonSerializer.Deserialize<List<PassiveData>>(magicJson, options);
		_passives.Add(PlayerClass.Magic, magicData);
		magicStream.Close();
		magicReader.Close();
		
		//Summoner
		Stream summonerStream = PathOfTerraria.Instance.GetFileStream("Data/Passives/SummonerPassives.json");
		using var summonerReader = new StreamReader(summonerStream);
		string summonerJson = summonerReader.ReadToEnd();
		List<PassiveData> summonerData = JsonSerializer.Deserialize<List<PassiveData>>(summonerJson, options);
		_passives.Add(PlayerClass.Summoner, summonerData);
		summonerStream.Close();
		summonerReader.Close();
	}
}
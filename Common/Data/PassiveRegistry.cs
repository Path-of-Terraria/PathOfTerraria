using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PathOfTerraria.Common.Data.FileWatchers;
using PathOfTerraria.Common.Data.Models;

namespace PathOfTerraria.Common.Data;

public class PassiveRegistry : ILoadable
{
	/// <summary>
	/// A map of MobData objects, with the key being the type from NPC.
	/// </summary>
	private static readonly List<PassiveData> Passives = [];

	private static JsonSerializerOptions Options { get; set; } = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};
	
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

	internal static void ClearData()
	{
		Passives.Clear();
	}
	
	/// <summary>
	/// Loads the JSON files from the paths.txt file and returns a map of the data.
	/// </summary>
	/// <returns></returns>
	internal static void LoadJsonFilesToMapAsync()
	{
		Stream passiveStream = GetPassiveJsonStream();
		using var passiveReader = new StreamReader(passiveStream);
		string passiveJson = passiveReader.ReadToEnd();
		List<PassiveData> passiveData = JsonSerializer.Deserialize<List<PassiveData>>(passiveJson, Options);

		passiveData // no clue how to handle empty values, lol
			.ForEach(d => {
				d.Connections ??= [];
				d.Position ??= new PassivePosition();
			});

		Passives.AddRange(passiveData);
		passiveStream.Close();
		passiveReader.Close();
	}

	/// <summary>
	/// Determines which file to stream to load passives. This either returns the internal Passives.json using <see cref="Mod.GetFileStream(string, bool)"/>,
	/// or the local ModSources/PathOfTerraria/Common/Data/Passives/Passives.json.<br/>
	/// This is used to load the more up to date local file when debugging if the developer is updating it.<br/>
	/// In release, always returns the internal stream.
	/// </summary>
	/// <returns>The stream pointing to the passive data json.</returns>
	private static Stream GetPassiveJsonStream()
	{
#if DEBUG
		string sourcePath = Path.Combine(PassiveFileWatcher.SourcePath, "Passives.json");
		string tmodPath = Path.Combine(ModLoader.ModPath, PoTMod.ModName + ".tmod");

		// If the mod was built later than the passives file was opened, open the internal file
		if (File.GetLastWriteTime(sourcePath).CompareTo(File.GetLastWriteTime(tmodPath)) <= 0)
		{
			return PoTMod.Instance.GetFileStream($"Common/Data/Passives/Passives.json");
		}

		// Otherwise, return the in-dev file
		return new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
#else
		// Release code always uses the internal file
		return PoTMod.Instance.GetFileStream($"Common/Data/Passives/Passives.json");
#endif
	}
}
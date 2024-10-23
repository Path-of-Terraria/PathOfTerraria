using PathOfTerraria.Common.UI;
using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using System.IO;

namespace PathOfTerraria.Common.Data.FileWatchers;

internal class PassiveFileWatcher : ModSystem
{
	private const int MaxWaitCooldown = 60;

	/// <summary>
	/// Points to the Passives folder in Common/Data/Passives.
	/// </summary>
	public static string SourcePath => Path.Combine(Program.SavePathShared, "ModSources", PoTMod.ModName, "Common", "Data", "Passives");

	private static readonly HashSet<string> _updatedFiles = [];

	private static FileSystemWatcher _watcher;
	private static int _watcherCooldown = 0;

	public override void Load()
	{
		string path = Path.Combine(Program.SavePathShared, "ModSources", PoTMod.ModName, "Common", "Data", "Passives");

		if (!Directory.Exists(path))
		{
			return;
		}

		_watcherCooldown = 0;
		_watcher = new FileSystemWatcher
		{
			Path = path,
			NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
			Filter = "*.json",
			IncludeSubdirectories = true
		};

		_watcher.Changed += (object a, FileSystemEventArgs b) => HandleFileChangedOrRenamed(b.Name);
		_watcher.Renamed += (object a, RenamedEventArgs b) => HandleFileChangedOrRenamed(b.Name);
		_watcher.EnableRaisingEvents = true;
	}

	/// <summary>
	/// Unsure if this is the best hook to use, but it works.
	/// </summary>
	public override void PreUpdateEntities()
	{
		_watcherCooldown--;

		// If a file was just changed, update the passives and reload the UI if necessary.
		if (_watcherCooldown == MaxWaitCooldown - 1)
		{
			string files = string.Empty;

			foreach (string file in _updatedFiles)
			{
				files += file + " ";
			}

			Main.NewText($"Reloaded data, file(s) updated: {files}.");
			_updatedFiles.Clear();

			PassiveRegistry.ClearData();

			if (SmartUiLoader.GetUiState<TreeState>().IsVisible)
			{
				SmartUiLoader.GetUiState<TreeState>().Toggle();
				PassiveRegistry.LoadJsonFilesToMapAsync();
				SmartUiLoader.GetUiState<TreeState>().Toggle();
			}
			else
			{
				PassiveRegistry.LoadJsonFilesToMapAsync();
			}
		}
	}

	private static void HandleFileChangedOrRenamed(string name)
	{
		_watcherCooldown = MaxWaitCooldown;
		_updatedFiles.Add(name);
	}
}

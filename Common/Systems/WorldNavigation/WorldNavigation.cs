using System.IO;
using Terraria.IO;

namespace PathOfTerraria.Common.Systems.WorldNavigation;

public abstract class WorldNavigation : ModSystem
{
	protected virtual string WorldFileName => string.Empty;
	public virtual bool IsWorldLoaded { get; set; }
	public virtual Vector2 SpawnPosition => new();

	public override void Load()
	{
		if (!File.Exists(Path.Combine(ModLoader.ModPath, $"{WorldFileName}.wld")) || !File.Exists(Path.Combine(ModLoader.ModPath, $"{WorldFileName}.twld")))
		{
			File.WriteAllBytes(Path.Combine(ModLoader.ModPath, $"{WorldFileName}.wld"), Mod.GetFileBytes($"Assets/Worlds/{WorldFileName}.wld"));
			File.WriteAllBytes(Path.Combine(ModLoader.ModPath, $"{WorldFileName}.twld"), Mod.GetFileBytes($"Assets/Worlds/{WorldFileName}.twld"));
		}
	}

	public virtual void LoadWorld()
	{
		IsWorldLoaded = true;
		var worldData = new WorldFileData(ModLoader.ModPath + $"/{WorldFileName}.wld", false);
		worldData.SetAsActive();
		Main.ActiveWorldFileData = worldData;
		WorldGen.playWorld();
	}
}
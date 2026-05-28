using PathOfTerraria.Content.Swamp;
using SubworldLibrary;
using System.IO;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Swamp;

internal class SwampSystem : ModSystem
{
	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (SubworldSystem.Current is SwampArea)
		{
			tileColor = new Color(50, 70, 75);
			backgroundColor = new Color(100, 130, 135);
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		// Persist LeftSpawn into the swamp's own .twld so ArenaMiddleX is correct after process
		// restart (otherwise the static would default to false and arena geometry flips).
		if (SubworldSystem.Current is SwampArea)
		{
			tag["LeftSpawn"] = SwampArea.LeftSpawn;
		}
	}

	public override void LoadWorldData(TagCompound tag)
	{
		if (SubworldSystem.Current is SwampArea && tag.ContainsKey("LeftSpawn"))
		{
			SwampArea.LeftSpawn = tag.GetBool("LeftSpawn");
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		if (SubworldSystem.Current is not SwampArea)
		{
			return;
		}

		writer.Write(SwampArea.LeftSpawn);
	}

	public override void NetReceive(BinaryReader reader)
	{
		if (SubworldSystem.Current is not SwampArea)
		{
			return;
		}

		SwampArea.LeftSpawn = reader.ReadBoolean();
	}
}

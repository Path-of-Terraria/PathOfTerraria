using PathOfTerraria.Content.Swamp;
using SubworldLibrary;
using System.IO;

namespace PathOfTerraria.Content.Swamp;

internal class SwampSystem : ModSystem
{
	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (SubworldSystem.Current is SwampArea)
		{
			tileColor = new Color(140, 170, 180);
			backgroundColor = new Color(140, 170, 180);
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

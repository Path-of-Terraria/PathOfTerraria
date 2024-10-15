using SubworldLibrary;
using System.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

/// <summary>
/// Used to clamp camera to hide the other side of the domain for a "wow" factor.
/// </summary>
public class WoFDomainSystem : ModSystem
{
	public override void ModifyScreenPosition()
	{
		if (SubworldSystem.Current is WallOfFleshDomain domain && !domain.BossSpawned)
		{
			if (domain.LeftBlocked)
			{
				if (Main.screenPosition.X / 16f < Main.spawnTileX - 70)
				{
					Main.screenPosition.X = (Main.spawnTileX - 70) * 16;
				}
			}
			else
			{
				if ((Main.screenPosition.X + Main.screenWidth) / 16f > Main.spawnTileX + 70)
				{
					Main.screenPosition.X = (Main.spawnTileX + 70) * 16 - Main.screenWidth;
				}
			}
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		if (SubworldSystem.Current is WallOfFleshDomain domain && !domain.BossSpawned)
		{
			writer.Write(domain.LeftBlocked);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		if (SubworldSystem.Current is WallOfFleshDomain domain && !domain.BossSpawned)
		{
			domain.LeftBlocked = reader.ReadBoolean();
		}
	}
}
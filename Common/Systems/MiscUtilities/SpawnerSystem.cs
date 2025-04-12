using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.MiscUtilities;

/// <summary> Handles spawner tile reset. Not normally used by multiplayer clients. </summary>
internal class SpawnerSystem : ModSystem
{
	public readonly record struct TileData(int Type, short FrameX, Point16 Coords)
	{
		public readonly int Type = Type;
		public readonly short FrameX = FrameX;
		public readonly Point16 Coords = Coords;
	}

	/// <summary> Keeps a record of all consumed spawner tiles for reset. </summary>
	public static readonly Dictionary<int, TileData> SpawnerRecord = [];

	/// <summary> Resets a spawner corresponding to an NPC index. </summary>
	public static void ResetSpawner(int key)
	{
		ResetSingle(key);
		SpawnerRecord.Remove(key);
	}

	public static void ResetSpawners()
	{
		foreach (int key in SpawnerRecord.Keys)
		{
			ResetSingle(key);
		}

		SpawnerRecord.Clear();
	}

	private static void ResetSingle(int key)
	{
		TileData record = SpawnerRecord[key];

		WorldGen.PlaceTile(record.Coords.X, record.Coords.Y, record.Type, true);
		Framing.GetTileSafely(record.Coords).TileFrameX = record.FrameX;

		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			NetMessage.SendTileSquare(-1, record.Coords.X, record.Coords.Y);
		}
	}

	public override void ClearWorld()
	{
		SpawnerRecord.Clear();
	}

	public override void PostUpdateNPCs()
	{
		foreach (int key in SpawnerRecord.Keys)
		{
			if (!Main.npc[key].active)
			{
				SpawnerRecord.Remove(key); //Silently remove records if the NPC dies, for example
			}
		}
	}
}
using PathOfTerraria.Common.World.Generation;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.StructureImprovementSystem;

/// <summary>
/// Defines a structure that can be improved and persists somewhat.
/// </summary>
/// <param name="maxIndex">Max index of the structure.</param>
internal class ImprovableStructure(int maxIndex)
{
	public int StructureIndex { get; set; }

	public readonly int MaxIndex = maxIndex;

	public string StructurePath = string.Empty;
	public Point Position = new();

	public void Change(int newIndex)
	{
		StructureIndex = newIndex;

		if (StructureIndex > MaxIndex)
		{
			StructureIndex = MaxIndex;
		}
	}

	/// <summary>
	/// Places the structure at <see cref="Position"/>, reframes the area, and syncs if on server.
	/// </summary>
	public void Place()
	{
		Point16 pos = StructureTools.PlaceByOrigin(StructurePath + StructureIndex, new Point16(Position.X, Position.Y), Vector2.Zero);
		Point16 size = StructureTools.GetSize(StructurePath + StructureIndex);

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendTileSquare(-1, Position.X, Position.Y, size.X, size.Y);
		}

		for (int i = pos.X - 1; i < pos.X + size.X + 1; ++i)
		{
			for (int j = pos.Y - 1; j < pos.Y + size.Y + 1; ++j)
			{
				WorldGen.TileFrame(i, j, true);
			}
		}
	}
}

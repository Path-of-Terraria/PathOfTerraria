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

	public void Place()
	{
		StructureTools.PlaceByOrigin(StructurePath + StructureIndex, new Point16(Position.X, Position.Y), Vector2.Zero);

		if (Main.netMode == NetmodeID.Server)
		{
			Point16 size = StructureTools.GetSize(StructurePath + StructureIndex);
			NetMessage.SendTileSquare(-1, Position.X, Position.Y, size.X, size.Y);
		}
	}
}

using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.Passes.CaveSystemPasses;
internal class SpawnAndBossPositionPass() : GenPass("BossAndSpawnRooms", 1)
{
	private Vector2 GetEdgePosition(float r, int height, int widht)
	{
		float normalR = (r + MathF.PI * 1.25f) % (MathF.PI * 2f);
		int edgeSide = (int)(normalR / (MathF.PI / 2));

		return edgeSide switch
		{
			0 => new(-widht, widht * MathF.Tan(-r)),// left edge
			1 => new(height / MathF.Tan(-r), -height),// down edge
			2 => new(widht, widht * MathF.Tan(r)),// right edge
			_ => new(height / MathF.Tan(r), height),// up edge
		};
	}

	const float allowableDivergence = 0.3f;
	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
	{
		float orientation = Main.rand.NextFloat() * MathF.PI * 2;

		float playerR = orientation + (Main.rand.NextFloat() * 2f - 1f) * allowableDivergence;
		float bossR = MathF.PI + orientation + (Main.rand.NextFloat() * 2f - 1f) * allowableDivergence;

		int width = Main.maxTilesX / 2;
		int height = Main.maxTilesY / 2;

		int widthP = width - CaveSystemWorld.Map.SpawnRoomSize; // - some extra amount; // bc terraria is dumb, it'd seem
		int heightP = height - CaveSystemWorld.Map.SpawnRoomSize;

		int widthB = width - CaveSystemWorld.Map.BossRoomSize;
		int heightB = height - CaveSystemWorld.Map.BossRoomSize;

		Vector2 baseHalfSize = new Vector2(width, height);
		Vector2 playerEdgePos = GetEdgePosition(playerR, widthP, heightP);
		Vector2 bossEdgePos = GetEdgePosition(bossR, widthB, heightB);

		Vector2 spawnPos = baseHalfSize + playerEdgePos;

		CaveSystemWorld.AddRoom(baseHalfSize + playerEdgePos, CaveSystemWorld.Map.SpawnRoomSize);
		
		CaveSystemWorld.AddRoom(baseHalfSize + bossEdgePos, CaveSystemWorld.Map.BossRoomSize);

		Main.spawnTileX = (int)spawnPos.X;
		Main.spawnTileY = (int)spawnPos.Y;
	}
}

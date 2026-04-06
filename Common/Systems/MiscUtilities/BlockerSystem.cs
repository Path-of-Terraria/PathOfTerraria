using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.MiscUtilities;

/// <summary>
/// Defines a tile as a "blocker" tile - nonsolid, unless any "arena enemies" (<see cref="ArenaEnemyNPC"/>) are alive.
/// </summary>
public interface IBlockerTile { }

public class BlockerSystem : ModSystem
{
	public static int[] BlockerTiles { get; private set; }

	internal static float FadeOut = 0;
	internal static bool HasArenaEnemies = false;

	public override void PostSetupContent()
	{
		List<int> blockers = [];

		for (int i = TileID.Count; i < TileLoader.TileCount; ++i)
		{
			ModTile tile = ModContent.GetModTile(i);

			if (tile is IBlockerTile)
			{
				blockers.Add(tile.Type);
			}
		}

		BlockerTiles = blockers.ToArray();
	}

	public override void PreUpdatePlayers()
	{
		FadeOut = MathHelper.Lerp(FadeOut, !HasArenaEnemies ? 0 : 1, 0.06f);
		HasArenaEnemies = false;

		// Fixes issue where during hardmode worldgen, this throws.
		if (SubworldSystem.Current is null)
		{
			SetBlockerSolidity(false);
			return;
		}

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.TryGetGlobalNPC(out ArenaEnemyNPC enemy) && enemy.Arena)
			{
				HasArenaEnemies = true;
				break;
			}
		}

		SetBlockerSolidity();
	}

	private static void SetBlockerSolidity(bool? overrideValue = null)
	{
		foreach (int type in BlockerTiles)
		{
			Main.tileSolid[type] = overrideValue ?? HasArenaEnemies;
		}
	}

	public static void DrawGlow(int i, int j, int type, SpriteBatch spriteBatch, Texture2D texture, Color fadeColor)
	{
		if (FadeOut > 0)
		{
			Tile tile = Main.tile[i, j];
			Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Vector2 position = new Vector2(i, j).ToWorldCoordinates(0, 0) - Main.screenPosition + offScreen;
			var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
			float colorAmount = MathHelper.Lerp(MathF.Sin(i + j + Main.GameUpdateCount * 0.05f), 1, 0.5f);
			float fade = 0.5f * FadeOut;

			if (!Main.tile[i, j - 1].HasTile || Main.tile[i, j - 1].TileType != type)
			{
				fade *= 0.5f;
			}
			else if (!Main.tile[i, j + 1].HasTile || Main.tile[i, j + 1].TileType != type)
			{
				fade *= 0.5f;
			}

			spriteBatch.Draw(texture, position, source, Color.Lerp(Color.White, fadeColor, colorAmount) * fade);
		}
	}
}

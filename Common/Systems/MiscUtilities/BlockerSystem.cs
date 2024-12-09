using PathOfTerraria.Common.Subworlds.BossDomains.WoFDomain;
using PathOfTerraria.Content.Tiles.BossDomain;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.MiscUtilities;

public class BlockerSystem : ModSystem
{
	internal static float FadeOut = 0;
	internal static bool HasArenaEnemies = false;

	public override void PreUpdatePlayers()
	{
		// Fixes issue where during hardmode worldgen, this throws.
		if (SubworldSystem.Current is null)
		{
			return;
		}

		HasArenaEnemies = false;

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.TryGetGlobalNPC(out ArenaEnemyNPC enemy) && enemy.Arena)
			{
				HasArenaEnemies = true;
				break;
			}
		}

		Main.tileSolid[ModContent.TileType<HiveBlocker>()] = HasArenaEnemies;
		Main.tileSolid[ModContent.TileType<ArenaBlocker>()] = HasArenaEnemies;

		if (HasArenaEnemies)
		{
			FadeOut = MathHelper.Lerp(FadeOut, 1, 0.06f);
		}
		else
		{
			FadeOut = MathHelper.Lerp(FadeOut, 0, 0.06f);
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

using PathOfTerraria.Common.Subworlds.BossDomains.WoFDomain;
using ReLogic.Content;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class ArenaBlocker : ModTile
{
	public static Asset<Texture2D> GlowTexture = null;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;

		Main.tileMerge[Type][TileID.ObsidianBrick] = true;
		Main.tileMerge[TileID.ObsidianBrick][Type] = true;

		AddMapEntry(new Color(175, 56, 76));

		DustType = DustID.Obsidian;
		HitSound = SoundID.NPCHit1;

		GlowTexture ??= ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		BlockerSystem.DrawGlow(i, j, Type, spriteBatch, GlowTexture.Value, Color.Red);
	}
}

internal class HiveBlocker : ModTile
{
	public static Asset<Texture2D> GlowTexture = null;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;

		Main.tileMerge[Type][TileID.Hive] = true;
		Main.tileMerge[TileID.Hive][Type] = true;

		AddMapEntry(new Color(175, 56, 76));

		DustType = DustID.Hive;
		HitSound = SoundID.NPCHit1;

		GlowTexture ??= ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		BlockerSystem.DrawGlow(i, j, Type, spriteBatch, GlowTexture.Value, Color.Orange);
	}
}

public class BlockerSystem : ModSystem
{
	public static float FadeOut = 0;

	public override void PreUpdateWorld()
	{
		bool anyArenas = false;

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.GetGlobalNPC<ArenaEnemyNPC>().Arena)
			{
				anyArenas = true;
				break;
			}
		}

		Main.tileSolid[ModContent.TileType<HiveBlocker>()] = anyArenas;
		Main.tileSolid[ModContent.TileType<ArenaBlocker>()] = anyArenas;

		if (anyArenas)
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

using PathOfTerraria.Common.Systems.MiscUtilities;
using ReLogic.Content;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Desert;

internal class SandstoneBrickBlocker : ModTile
{
	public static Asset<Texture2D> GlowTexture = null;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileBrick[Type] = true;

		Main.tileMerge[Type][TileID.ObsidianBrick] = true;
		Main.tileMerge[TileID.ObsidianBrick][Type] = true;

		AddMapEntry(new Color(138, 90, 10));

		DustType = DustID.Obsidian;
		GlowTexture ??= ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		BlockerSystem.DrawGlow(i, j, Type, spriteBatch, GlowTexture.Value, Color.Red);
	}
}
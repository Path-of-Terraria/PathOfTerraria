using PathOfTerraria.Common.Systems.MiscUtilities;
using ReLogic.Content;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Forest;

internal class LivingWoodBlocker : ModTile
{
	public static Asset<Texture2D> GlowTexture = null;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;

		Main.tileMerge[Type][TileID.LivingWood] = true;
		Main.tileMerge[TileID.LivingWood][Type] = true;

		AddMapEntry(new Color(169, 104, 57));

		DustType = DustID.Obsidian;
		GlowTexture ??= ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		BlockerSystem.DrawGlow(i, j, Type, spriteBatch, GlowTexture.Value, Color.RosyBrown);
	}
}

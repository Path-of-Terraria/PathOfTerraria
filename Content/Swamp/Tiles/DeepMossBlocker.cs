using PathOfTerraria.Common.Systems.MiscUtilities;
using PathOfTerraria.Content.Dusts;
using ReLogic.Content;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class DeepMossBlocker : ModTile, IBlockerTile
{
	public static Asset<Texture2D> GlowTexture = null;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileBrick[Type] = true;

		AddMapEntry(new Color(52, 114, 73));

		DustType = ModContent.DustType<DarkMossDust>();

		GlowTexture ??= ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		BlockerSystem.DrawGlow(i, j, Type, spriteBatch, GlowTexture.Value, Color.Orange);
	}
}
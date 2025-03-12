using PathOfTerraria.Common.Tiles;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class LaserBlock : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;
		Main.tileLighted[Type] = true;

		TileID.Sets.DrawsWalls[Type] = true;

		AddMapEntry(Color.Red);

		DustType = DustID.Firework_Red;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		float sine = MathF.Max(0, 3 * MathF.Sin(-i - j + 0.15f * Main.GameUpdateCount)) * 0.5f;

		(r, g, b) = (0.5f * sine, 0.04f * sine, 0.06f * sine);
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Vector2 pos = TileExtensions.DrawPosition(i, j);
		Tile tile = Main.tile[i, j];
		Rectangle source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
		float sine = MathF.Max(0, 3 * MathF.Sin(-i - j + 0.15f * Main.GameUpdateCount)) * 0.5f + 0.5f;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, pos, source, Color.White * sine, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);

		return false;
	}
}
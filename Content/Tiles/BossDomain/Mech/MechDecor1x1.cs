using PathOfTerraria.Common.Tiles;
using ReLogic.Content;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class MechDecor1x1 : ModTile
{
	private static Asset<Texture2D> Glow = null;

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");

		Main.tileCut[Type] = true;
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateHeights = [18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 8;
		TileObjectData.newTile.AnchorBottom = new(TileObjectData.newTile.AnchorBottom.type | Terraria.Enums.AnchorType.PlatformNonHammered, 1, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Iron;

		AddMapEntry(new Color(128, 128, 128));
	}

	public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
	{
		spriteEffects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (Main.tile[i, j].TileFrameX / 16 is not 1 and not 7)
		{
			return;
		}

		Vector2 pos = TileExtensions.DrawPosition(i, j);
		Rectangle source = TileExtensions.BasicFrame(i, j);
		float sine = MathF.Max(0, 3 * MathF.Sin(-i - j + 0.05f * Main.GameUpdateCount)) * 0.5f;

		spriteBatch.Draw(Glow.Value, pos, source, Color.White * sine, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
	}
}

using PathOfTerraria.Common.Tiles;
using ReLogic.Content;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class MechDecor2x2 : ModTile
{
	private static Asset<Texture2D> Glow = null;

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");

		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 4;
		TileObjectData.newTile.Origin = new Terraria.DataStructures.Point16(0, 1);
		TileObjectData.newTile.AnchorBottom = new(TileObjectData.newTile.AnchorBottom.type | Terraria.Enums.AnchorType.PlatformNonHammered, 2, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Iron;

		AddMapEntry(new Color(128, 128, 128));
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Vector2 pos = TileExtensions.DrawPosition(i, j);
		Rectangle source = TileExtensions.BasicFrame(i, j);
		float sine = MathF.Max(0, 3 * MathF.Sin(-i - j + 0.05f * Main.GameUpdateCount)) * 0.5f;

		spriteBatch.Draw(Glow.Value, pos, source, Color.White * sine, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
	}
}

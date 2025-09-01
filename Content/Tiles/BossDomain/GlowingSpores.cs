using PathOfTerraria.Common.Tiles;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class GlowingSpores : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.AnchorWall = true;
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		DustType = DustID.CorruptGibs;

		AddMapEntry(new Color(102, 115, 15));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];
		int frameX = tile.TileFrameX / 18 % 3;

		(r, g, b) = frameX switch
		{
			0 => (0.4f, 0.5f, 0.1f),
			1 => (0.1f, 0.6f, 0.15f),
			_ => (0.3f, 0.6f, 0.25f)
		};
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Texture2D tex = TextureAssets.Tile[Type].Value;
		Tile tile = Main.tile[i, j];
		var src = new Rectangle(tile.TileFrameX % 54, tile.TileFrameY, 16, 16);
		Vector2 position = TileExtensions.DrawPosition(i, j) + new Vector2(8, 8);
		float sine = MathF.Sin(i + j + (float)Main.timeForVisualEffects * 0.01f) * MathHelper.PiOver4;

		spriteBatch.Draw(tex, position, src, Lighting.GetColor(i, j), sine, src.Size() / 2f, 1f, SpriteEffects.None, 0);
		return false;
	}
}
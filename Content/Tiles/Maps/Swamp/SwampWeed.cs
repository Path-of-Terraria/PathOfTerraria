using PathOfTerraria.Common.Tiles;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Swamp;

internal class SwampWeed : ModTile
{
	public static void Place(int i, int j, int frame)
	{
		Tile weed = Main.tile[i, j];

		if (weed.HasTile || weed.LiquidAmount < 200)
		{
			return;
		}

		weed.HasTile = true;
		weed.TileType = (ushort)ModContent.TileType<SwampWeed>();
		weed.TileFrameX = (short)(18 * frame);
		weed.TileFrameNumber = Main.rand.NextBool(70) ? 1 : 0;
		WorldGen.TileFrame(i, j);
	}

	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileLighted[Type] = true;

		DustType = DustID.Grass;

		AddMapEntry(new Color(182, 175, 130));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		if (Main.tile[i, j].TileFrameNumber != 0)
		{
			(r, g, b) = (0.2f, 0.2f, 0.05f);
		}
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		Tile above = Main.tile[i, j];

		if (!above.HasTile || above.TileType != Type)
		{
			tile.TileFrameY = (short)(54 + 18 * Main.rand.Next(2));
		}
		else
		{
			tile.TileFrameY = (short)(18 * Main.rand.Next(3));
		}

		return false;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (Main.tile[i, j].TileFrameNumber != 0)
		{
			Vector2 position = TileExtensions.DrawPosition(i, j);
			spriteBatch.Draw(TextureAssets.Tile[Type].Value, position.Floor(), new Rectangle(56, 18 * ((i + j) % 3), 16, 16), Color.White);
		}
	}
}

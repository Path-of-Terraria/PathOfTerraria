using PathOfTerraria.Common.Tiles;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class MechGate : ModTile
{
	private static Asset<Texture2D> Glow = null;

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");

		Main.tileFrameImportant[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateHeights = [18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.SolidSide | AnchorType.AlternateTile, 1, 0);
		TileObjectData.newTile.AnchorAlternateTiles = [Type];

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidSide | AnchorType.AlternateTile, 1, 0);
		TileObjectData.addAlternate(1);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidSide | AnchorType.AlternateTile, 1, 0);
		TileObjectData.addAlternate(2);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidSide | AnchorType.AlternateTile, 1, 0);
		TileObjectData.addAlternate(3);
		TileObjectData.addTile(Type);

		DustType = DustID.Iron;

		AddMapEntry(new Color(128, 128, 128));
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		
		if (Main.tile[i, j + 1].HasTile || Main.tile[i, j - 1].HasTile)
		{
			tile.TileFrameY = 18;
		}
		else
		{
			tile.TileFrameY = 0;
		}

		tile.TileFrameX = (short)(Main.rand.Next(3) * 18);
		return false;
	}

	public override void HitWire(int i, int j)
	{
		WorldGen.KillTile(i, j);

		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			NetMessage.SendTileSquare(-1, i, j);
		}
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

		Rectangle frame = tile.BasicFrame();
		frame.X %= 36;
		spriteBatch.Draw(TextureAssets.Tile[Type].Value, TileExtensions.DrawPosition(i, j), frame, Lighting.GetColor(i, j));

		if (tile.HasUnactuatedTile)
		{
			spriteBatch.Draw(Glow.Value, TileExtensions.DrawPosition(i, j), frame, Color.White);
		}

		return false;
	}
}

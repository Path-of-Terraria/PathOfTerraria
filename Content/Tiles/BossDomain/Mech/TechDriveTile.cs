using PathOfTerraria.Common.Systems.DisableBuilding;
using PathOfTerraria.Common.Tiles;
using PathOfTerraria.Content.Items.BossDomain;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class TechDriveTile : ModTile, ICanCutTile
{
	private static Asset<Texture2D> Glow = null;

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");

		Main.tileCut[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileMerge[TileID.Chain][Type] = true;
		Main.tileMerge[Type][TileID.Chain] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateHeights = [16];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorTop = new(Terraria.Enums.AnchorType.SolidBottom | Terraria.Enums.AnchorType.AlternateTile, 1, 0);
		TileObjectData.newTile.AnchorAlternateTiles = [TileID.Chain];
		TileObjectData.addTile(Type);

		DustType = DustID.Iron;

		AddMapEntry(new Color(172, 2, 5));
		RegisterItemDrop(ModContent.ItemType<MechDrive>());
	}

	bool ICanCutTile.CanCut(int i, int j)
	{
		return Main.tile[i, j].TileFrameY > 0;
	}

	public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
	{
		spriteEffects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
	}

	public override void HitWire(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		tile.TileFrameY = 18;

		if (Main.dedServ)
		{
			NetMessage.SendTileSquare(-1, i, j);
		}
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameY == 0)
		{
			return true;
		}

		spriteBatch.Draw(Glow.Value, TileExtensions.DrawPosition(i, j) + new Vector2(8), null, Color.White, 0f, Glow.Size() / 2f, 1f, SpriteEffects.None, 0);
		return true;
	}
}

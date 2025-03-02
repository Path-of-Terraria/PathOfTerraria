using PathOfTerraria.Common.Tiles;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class MechButton : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.IsATrigger[Type] = true;
		TileID.Sets.IsAMechanism[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.Width = 3;
		TileObjectData.newTile.CoordinateHeights = [18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.Origin = new Point16(1, 0);
		TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.SolidTile | Terraria.Enums.AnchorType.SolidWithTop, 3, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Iron;

		AddMapEntry(new Color(128, 128, 128));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
		{
			return;
		}

		bool anyPlayerOn = false;

		foreach (Player player in Main.ActivePlayers)
		{
			if (!player.dead && Math.Abs(player.Center.X - (i + 1) * 16) < 32 + player.width && Math.Abs(player.Bottom.Y - (j + 0.5f) * 16) < 14)
			{
				anyPlayerOn = true;
				break;
			}
		}

		if (anyPlayerOn)
		{
			short frameAdjustment = 54;

			for (int k = 0; k < 3; ++k)
			{
				Main.tile[i + k, j].TileFrameX += frameAdjustment;
			}

			Wiring.TripWire(i, j, 3, 1);

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendTileSquare(-1, i, j, 3, 1);
				NetMessage.SendData(MessageID.HitSwitch, -1, Main.myPlayer, null, i, j);
				NetMessage.SendData(MessageID.HitSwitch, -1, Main.myPlayer, null, i + 1, j);
				NetMessage.SendData(MessageID.HitSwitch, -1, Main.myPlayer, null, i + 2, j);
			}
		}
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Rectangle frame = TileExtensions.BasicFrame(i, j) with { Y = 20 };
		spriteBatch.Draw(TextureAssets.Tile[Type].Value, TileExtensions.DrawPosition(i, j), frame, Color.White);
	}
}

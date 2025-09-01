using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class MechLamp : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileLighted[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileWaterDeath[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
		TileObjectData.newTile.WaterDeath = true;
		TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
		TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
		TileObjectData.newTile.Origin = new Terraria.DataStructures.Point16(0, 0);
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
		AddMapEntry(new Color(222, 210, 210), Language.GetText("MapObject.FloorLamp"));
	}

	public override void HitWire(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		int topY = j - tile.TileFrameY / 18 % 3;
		short frameAdjustment = (short)(tile.TileFrameX > 0 ? -18 : 18);
		Main.tile[i, topY].TileFrameX += frameAdjustment;
		Main.tile[i, topY + 1].TileFrameX += frameAdjustment;
		Main.tile[i, topY + 2].TileFrameX += frameAdjustment;
		Wiring.SkipWire(i, topY);
		Wiring.SkipWire(i, topY + 1);
		Wiring.SkipWire(i, topY + 2);
		NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
	}

	public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
	{
		spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
		{
			r = 1f;
			g = 0.3f;
			b = 0.3f;
		}
	}
}

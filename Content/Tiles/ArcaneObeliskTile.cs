using PathOfTerraria.Content.Items.Placeable;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles;

public class ArcaneObeliskTile : ModTile
{
	public override string Texture => base.Texture.Replace("Content", "Assets");

	public override void SetStaticDefaults()
	{
		Main.tileLighted[Type] = true;
		Main.tileLavaDeath[Type] = false;
		Main.tileWaterDeath[Type] = false;
		Main.tileBlockLight[Type] = true;
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;
		
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);

		TileObjectData.newTile.DrawYOffset = 4;
		
		TileObjectData.newTile.Height = 5;
		TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16 };
		
		TileObjectData.newTile.Origin = Point16.Zero;
		
		TileObjectData.addTile(Type);

		DustType = DustID.Stone;
		HitSound = SoundID.Dig;
		
		AddMapEntry(new Color(142, 136, 174), CreateMapEntryName());
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
	
	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
		b = 0.2f;
   	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
	{
		return true;
	}

	public override void MouseOver(int i, int j)
	{
		Main.LocalPlayer.cursorItemIconEnabled = true;
		Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<ArcaneObeliskItem>();
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		var tile = Framing.GetTileSafely(i, j);
		
		var frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 18, 18);
		var data = TileObjectData.GetTileData(tile);

		var texture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
		
		var dataOffset = new Vector2(data.DrawXOffset, data.DrawYOffset);
		var screenOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
		
		var position = new Vector2(i, j) * 16f - Main.screenPosition + screenOffset + dataOffset;
		
		spriteBatch.Draw(texture, position, frame, Color.White);
	}
}
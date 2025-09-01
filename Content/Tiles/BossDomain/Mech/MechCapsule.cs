using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Tiles;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class MechCapsule : ModTile
{
	private static Asset<Texture2D> Glow = null;

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");

		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 1;
		TileObjectData.newTile.Origin = new Terraria.DataStructures.Point16(0, 1);
		TileObjectData.newTile.AnchorBottom = new(TileObjectData.newTile.AnchorBottom.type | Terraria.Enums.AnchorType.PlatformNonHammered, 2, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Iron;

		AddMapEntry(new Color(128, 128, 128));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (0.6f, 0.1f, 0.1f);
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
	{
		return true;
	}

	public override bool RightClick(int i, int j)
	{
		WorldGen.KillTile(i, j);
		
		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			NetMessage.SendTileSquare(-1, i, j, 4);
		}

		return true;
	}

	public override IEnumerable<Item> GetItemDrops(int i, int j)
	{
		ItemDatabase.ItemRecord drop = DropTable.RollMobDrops(PoTItemHelper.PickItemLevel(), 1f, random: Main.rand);
		yield return new Item(drop.ItemId, drop.Item.stack);
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Vector2 pos = TileExtensions.DrawPosition(i, j);
		Rectangle source = TileExtensions.BasicFrame(i, j);
		float sine = MathF.Max(0, 3 * MathF.Sin(-i - j + 0.05f * Main.GameUpdateCount)) * 0.5f;

		spriteBatch.Draw(Glow.Value, pos, source, Color.White * sine, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
	}
}

using PathOfTerraria.Common.Tiles;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class PulseGenerator : ModTile
{
	private static Asset<Texture2D> Glow = null;
	private static int Timer = 0;

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");

		Main.tileCut[Type] = true;
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.Width = 6;
		TileObjectData.newTile.DrawYOffset = -2;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidBottom | AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Iron;

		AddMapEntry(new Color(128, 128, 128));
	}

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
		if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0)
		{
			Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
		}
	}

	public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		int timer = Timer + i * 3 + j * 2;

		if (tile.TileFrameX == 0 && tile.TileFrameY == 0 && timer % 18 == 0)
		{
			Gore.NewGore(new EntitySource_TileUpdate(i, j), new Vector2(i + 1.5f, j).ToWorldCoordinates(8, 2), Vector2.Zero, ModContent.GoreType<PulseEffect>(), 1);
		}
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Vector2 pos = TileExtensions.DrawPosition(i, j) - new Vector2(0, 2);
		Rectangle source = TileExtensions.BasicFrame(i, j);

		spriteBatch.Draw(Glow.Value, pos, source, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
	}

	public class PulseSystem : ModSystem
	{
		public static List<Point16> Pulses = [];

		public override void PreUpdateDusts()
		{
			Timer++;
		}
	}

	public class PulseEffect : ModGore
	{
		public override void OnSpawn(Gore gore, IEntitySource source)
		{
			gore.velocity.X = 0;
			gore.velocity.Y = 4;
		}

		public override bool Update(Gore gore)
		{
			UpdateState(gore);

			if (Collision.SolidCollision(gore.position, 32, 10))
			{
				UpdateState(gore);
				UpdateState(gore);
			}

			gore.position += gore.velocity;

			if (gore.scale <= 0)
			{
				gore.active = false;
			}

			return false;
		}

		private static void UpdateState(Gore gore)
		{
			gore.velocity.Y *= 0.97f;
			gore.scale -= 0.01f;
		}

		public override Color? GetAlpha(Gore gore, Color lightColor)
		{
			return Color.White * (MathF.Pow(MathF.Sin(gore.position.Y / 10f), 2) * 0.5f + 0.5f);
		}
	}
}

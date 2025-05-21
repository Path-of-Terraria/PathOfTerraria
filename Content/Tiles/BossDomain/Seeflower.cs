using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Tiles;
using ReLogic.Content;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class Seeflower : ModTile
{
	private static Asset<Texture2D> WorldTexture = null;

	public override void SetStaticDefaults()
	{
		WorldTexture = ModContent.Request<Texture2D>(Texture + "World");

		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);

		SeeflowerTE tileEntity = ModContent.GetInstance<SeeflowerTE>();
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);

		TileObjectData.newTile.RandomStyleRange = 1;
		TileObjectData.newTile.AnchorWall = true;
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		DustType = DustID.CorruptGibs;

		AddMapEntry(new Color(233, 235, 103));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (0.6f, 0.5f, 0.0f);
	}

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
		Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
	}

	public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Texture2D tex = WorldTexture.Value;
		Tile tile = Main.tile[i, j];
		Vector2 position = TileExtensions.DrawPosition(i - 11, j - 11) - new Vector2(8);
		float sine = MathF.Sin(i + j + (float)Main.timeForVisualEffects * 0.01f) * 0.2f;
		float angle = (TileEntity.ByPosition[new Point16(i, j)] as SeeflowerTE).Angle;
		
		spriteBatch.Draw(tex, position, null, Lighting.GetColor(i, j), angle + sine, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}

	public class SeeflowerTE : ModTileEntity
	{
		internal float Angle = 0;

		public override bool IsTileValidForEntity(int x, int y)
		{
			return Main.tile[x, y].TileType == ModContent.TileType<Seeflower>();
		}

		public override void Update()
		{
			float targetAngle = Position.ToWorldCoordinates().AngleTo(PlanteraDomain.BulbPosition.ToWorldCoordinates()) - 0.6f;
			Angle = Utils.AngleLerp(Angle, targetAngle, 0.01f);
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			var tileData = TileObjectData.GetTileData(type, style, alternate);
			int topLeftX = i - tileData.Origin.X;
			int topLeftY = j - tileData.Origin.Y;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, tileData.Width, tileData.Height);
				NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: Type);
				return -1;
			}

			return Place(topLeftX, topLeftY);
		}
	}
}
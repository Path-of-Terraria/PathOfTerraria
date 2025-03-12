using PathOfTerraria.Common.Tiles;
using PathOfTerraria.Content.NPCs.BossDomain.Mech;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class GrabberAnchor : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateHeights = [16];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 1;

		GrabberSpawnEntity tileEntity = ModContent.GetInstance<GrabberSpawnEntity>();
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);

		TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.PlatformNonHammered, 1, 0);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorTop = new(AnchorType.SolidBottom | AnchorType.SolidTile, 1, 0);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.addAlternate(1);
		
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorLeft = new(AnchorType.SolidSide | AnchorType.SolidTile, 1, 0);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.addAlternate(2);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorRight = new(AnchorType.SolidSide | AnchorType.SolidTile, 1, 0);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.addAlternate(3);

		TileObjectData.addTile(Type);

		DustType = DustID.Iron;

		AddMapEntry(new Color(128, 128, 128));
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		ModContent.GetInstance<GrabberSpawnEntity>().Kill(i, j);
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Vector2 pos = TileExtensions.DrawPosition(i, j) - new Vector2(2);
		Tile tile = Main.tile[i, j];
		Rectangle source = new Rectangle(tile.TileFrameX / 18 * 22, 18, 20, 20);
		float sine = MathF.Max(0, 3 * MathF.Sin(-i - j + 0.05f * Main.GameUpdateCount)) * 0.5f;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, pos, source, Color.White * sine, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
	}

	public class GrabberSpawnEntity : ModTileEntity
	{
		private bool _hasSpawnedEnemy = false;

		public override bool IsTileValidForEntity(int x, int y)
		{
			return Main.tile[x, y].TileType == ModContent.TileType<GrabberAnchor>();
		}

		public override void Update()
		{
			if (!_hasSpawnedEnemy)
			{
				_hasSpawnedEnemy = true;

				NPC.NewNPC(new EntitySource_SpawnNPC(), Position.X * 16, Position.Y * 16, ModContent.NPCType<Grabber>(), 0, Position.X * 16, Position.Y * 16, 0, -1);
			}
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

public class GrabberAnchorItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<GrabberAnchor>(), 0);
	}
}
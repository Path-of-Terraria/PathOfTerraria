using PathOfTerraria.Common.Subworlds.BossDomains.DeerDomain;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class PolarIceThrower : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 16];
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.newTile.StyleHorizontal = true;

		ThrowerTE tileEntity = ModContent.GetInstance<ThrowerTE>();
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.addAlternate(1);
		TileObjectData.addTile(Type);

		DustType = DustID.Ice;

		AddMapEntry(new Color(250, 250, 255));
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		ModContent.GetInstance<ThrowerTE>().Kill(i, j);
	}

	public class ThrowerTE : ModTileEntity
	{
		private int _timer = 14 * 16;

		public override bool IsTileValidForEntity(int x, int y)
		{
			return Main.tile[x, y].TileType == ModContent.TileType<PolarIceThrower>();
		}

		public override void Update()
		{
			_timer++;

			Vector2 worldPos = Position.ToWorldCoordinates();

			if (_timer > 16 * 60 && Main.player[Player.FindClosest(worldPos, 32, 32)].DistanceSQ(worldPos) < 400 * 400)
			{
				int side = Main.tile[Position].TileFrameX < 36 ? -1 : 1;
				Vector2 position = worldPos + new Vector2(side == -1 ? -20 : 36, 10);
				Projectile.NewProjectile(null, position, new Vector2(side * 1.2f, 0), ModContent.ProjectileType<LightBallProjectile>(), 40, 0, Main.myPlayer);
				_timer = 0;
			}
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			TileObjectData tileData = TileObjectData.GetTileData(type, style, alternate);
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
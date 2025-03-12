using PathOfTerraria.Common.NPCs;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class LaserShooter : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 16];
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.Origin = new Point16(0, 0);

		LaserShooterTE tileEntity = ModContent.GetInstance<LaserShooterTE>();
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.addAlternate(1);
		TileObjectData.addTile(Type);

		DustType = DustID.Ice;

		AddMapEntry(new Color(255, 120, 120));
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		ModContent.GetInstance<LaserShooterTE>().Kill(i, j);
	}

	public class LaserShooterTE : ModTileEntity
	{
		public const int MaxTimer = 12 * 60;

		internal int Timer = 0;

		public override bool IsTileValidForEntity(int x, int y)
		{
			return Main.tile[x, y].TileType == ModContent.TileType<LaserShooter>();
		}

		public override void Update()
		{
			Timer++;

			Vector2 worldPos = Position.ToWorldCoordinates();

			if (Timer > MaxTimer)
			{
				int side = Main.tile[Position].TileFrameX < 36 ? -1 : 1;
				Vector2 position = worldPos + new Vector2(side == -1 ? -20 : 36, 10);
				int damage = ModeUtils.ProjectileDamage(50, 80, 150);
				int proj = Projectile.NewProjectile(null, position, new Vector2(side * 7f, 0), ProjectileID.DeathLaser, damage, 0, Main.myPlayer);
				Main.projectile[proj].timeLeft = 5000;
				
				Timer = 0;
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
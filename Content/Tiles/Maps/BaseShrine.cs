using PathOfTerraria.Content.Items.Placeable;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Maps;

public enum ShrineType
{
	Forest = 0,
	Desert = 1,
}

public abstract class BaseShrine : ModTile
{
	public abstract int AoE { get; }

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileNoAttach[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;

		ShrineTileEntity tileEntity = ModContent.GetInstance<ShrineTileEntity>();
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleWrapLimit = 3;
		TileObjectData.newTile.Origin = new Point16(1, 0);
		TileObjectData.addTile(Type);

		AddMapEntry(Color.LightSlateGray);

		DustType = DustID.Stone;
		HitSound = SoundID.Dig;
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		ModContent.GetInstance<ShrineTileEntity>().Kill(i, j);
	}

	public virtual int Activate(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		int type = tile.TileFrameX / 36;
		return Projectile.NewProjectile(new EntitySource_SpawnNPC(), new Vector2(i, j).ToWorldCoordinates(12, 12), Vector2.Zero, AoE, 0, 0, Main.myPlayer, type);
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
	{
		return true;
	}

	public override void MouseOver(int i, int j)
	{
		base.MouseOver(i, j);

		Main.LocalPlayer.cursorItemIconEnabled = true;
		Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<ArcaneObeliskItem>();
	}

	public override bool RightClick(int i, int j)
	{
		WorldGen.KillTile(i, j);
		int buffId = (ContentSamples.ProjectilesByType[AoE].ModProjectile as ShrineAoE).BuffId;
		Main.LocalPlayer.AddBuff(buffId, 20 * 60);

		return true;
	}

	public class ShrineTileEntity : ModTileEntity
	{
		private int _projectile = -1;

		public override bool IsTileValidForEntity(int x, int y)
		{
			return ModContent.GetModTile(Main.tile[x, y].TileType) is BaseShrine;
		}

		public override void Update()
		{
			if (_projectile <= -1)
			{
				Tile tile = Main.tile[Position];
				var shrine = ModContent.GetModTile(tile.TileType) as BaseShrine;
				_projectile = shrine.Activate(Position.X, Position.Y);

				if (Main.projectile[_projectile].ModProjectile is not ShrineAoE)
				{
					throw new InvalidCastException("Baseshrine.Activate() should spawn a child of ShrineAoE!");
				}
			}
			else
			{
				Projectile projectile = Main.projectile[_projectile];

				if (!projectile.active || projectile.ModProjectile is not ShrineAoE)
				{
					_projectile = -1;
				}
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
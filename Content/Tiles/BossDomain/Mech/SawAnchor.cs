using PathOfTerraria.Common.Tiles;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class SawAnchor : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateHeights = [16];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 1;

		SawEntity tileEntity = ModContent.GetInstance<SawEntity>();
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

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.newAlternate.AnchorWall = true;
		TileObjectData.addAlternate(4);

		TileObjectData.addTile(Type);

		DustType = DustID.Iron;

		AddMapEntry(new Color(128, 128, 128));
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Vector2 pos = TileExtensions.DrawPosition(i, j) - new Vector2(2);
		Tile tile = Main.tile[i, j];
		Rectangle source = new Rectangle(tile.TileFrameX / 18 * 22, 18, 20, 20);
		float sine = MathF.Max(0, 3 * MathF.Sin(-i - j + 0.05f * Main.GameUpdateCount)) * 0.5f;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, pos, source, Color.White * sine, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
	}

	public class SawEntity : ModTileEntity
	{
		private bool _spawned = false;

		public override bool IsTileValidForEntity(int x, int y)
		{
			return Main.tile[x, y].TileType == ModContent.TileType<SawAnchor>();
		}

		public override void Update()
		{
			if (!_spawned)
			{
				_spawned = true;

				int type = ModContent.ProjectileType<SawProjectile>();
				Projectile.NewProjectile(new EntitySource_SpawnNPC(), Position.ToWorldCoordinates(), Vector2.Zero, type, 2, 0, Main.myPlayer, Main.rand.Next(2) + 2);
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

	public class SawProjectile : ModProjectile
	{
		ref float Size => ref Projectile.ai[0];
		ref float RotationSign => ref Projectile.ai[1];
		ref float RotationSpeed => ref Projectile.ai[2];

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.tileCollide = false;
			Projectile.timeLeft = 2;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.friendly = false;
		}

		public override void AI()
		{
			if (RotationSign == 0)
			{
				RotationSign = Main.rand.NextBool() ? -1 : 1;
				RotationSpeed = Main.rand.NextFloat(0.28f, 0.33f);
			}

			Projectile.timeLeft = 2;
			Projectile.rotation += RotationSpeed * RotationSign;
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			hitbox.Width = hitbox.Height = GetSize() - 10;
			hitbox.X -= hitbox.Width / 2;
			hitbox.Y -= hitbox.Height / 2;
		}

		private int GetSize()
		{
			return Size switch
			{
				0 => 38,
				1 => 58,
				2 => 66,
				_ => 112,
			};
		}

		public override bool PreDraw(ref Color lightColor)
		{
			int size = GetSize();
			int frameX = Size switch
			{
				0 => 0,
				1 => 40,
				2 => 100,
				_ => 168,
			};

			Rectangle frame = new(frameX, 0, size, size);
			Texture2D tex = TextureAssets.Projectile[Type].Value;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2f, 1f, SpriteEffects.None, 0);
			return false;
		}
	}
}

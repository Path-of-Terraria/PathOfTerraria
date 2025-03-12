using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

public class CannonAnchor : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateHeights = [16];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 1;

		CannonTE tileEntity = ModContent.GetInstance<CannonTE>();
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

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		ModContent.GetInstance<CannonTE>().Kill(i, j);
	}

	public class ExplosiveTurret : ModProjectile
	{
		public bool BombCannon
		{
			get => Projectile.ai[0] == 1;
			set => Projectile.ai[0] = value ? 1 : 0;
		}

		private ref float ShootTimer => ref Projectile.ai[1];
		private ref float Blowback => ref Projectile.ai[2];

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.tileCollide = false;
			Projectile.timeLeft = 2;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.width = 6;
			Projectile.height = 6;
		}

		public override void AI()
		{
			Projectile.timeLeft = 2;

			int targetWho = Player.FindClosest(Projectile.position, Projectile.width, Projectile.height);
			Player target = Main.player[targetWho];

			Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.AngleTo(target.Center + target.velocity * 20) - MathHelper.PiOver2, 0.12f);

			Blowback = MathHelper.Lerp(Blowback, 0, 0.07f);
			ShootTimer++;

			if (ShootTimer > 60)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient && SubworldSystem.Current is not null)
				{
					int type = BombCannon ? ProjectileID.Bomb : ProjectileID.RocketII;
					Vector2 vel = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 8;
					int damage = 90;
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + vel * 2, vel, type, damage, 0, Main.myPlayer);

					Blowback = 8;
				}

				ShootTimer = 0;
			}
		}

		public override bool CanHitPlayer(Player target)
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Rectangle frame = new(0, 0, 22, 22);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2f, 1f, SpriteEffects.None, 0);
			
			frame = new(BombCannon ? 28 : 0, 26, 26, 44);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, new Vector2(13, -6 + Blowback), 1f, SpriteEffects.None, 0);
			return false;
		}
	}
}

public class CannonBombAnchor : CannonAnchor
{
}

public class CannonTE : ModTileEntity
{
	private bool _spawned = false;

	public override bool IsTileValidForEntity(int x, int y)
	{
		return Main.tile[x, y].TileType == ModContent.TileType<CannonAnchor>() || Main.tile[x, y].TileType == ModContent.TileType<CannonBombAnchor>();
	}

	public override void Update()
	{
		if (!_spawned)
		{
			_spawned = true;

			int type = ModContent.ProjectileType<CannonAnchor.ExplosiveTurret>();
			Tile tile = Main.tile[Position];
			int bomb = tile.TileType == ModContent.TileType<CannonBombAnchor>() ? 1 : 0;
			Projectile.NewProjectile(new EntitySource_SpawnNPC(), Position.ToWorldCoordinates(), Vector2.Zero, type, 0, 0, Main.myPlayer, bomb, Main.rand.Next(120));
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

public class CannonAnchorItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<CannonAnchor>(), 0);
	}
}

public class CannonBombAnchorItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<CannonBombAnchor>(), 0);
	}
}

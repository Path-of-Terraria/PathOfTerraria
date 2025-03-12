using PathOfTerraria.Common.Tiles;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

public class ConstantLaser : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		this.MergeWith(TileID.CopperPlating, TileID.TinPlating, TileID.TinBrick, TileID.TungstenBrick, TileID.LeadBrick);

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.Origin = new Point16(0, 0);

		ConstantLaserTE tileEntity = ModContent.GetInstance<ConstantLaserTE>();
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

		AddMapEntry(new Color(250, 250, 255));
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		ModContent.GetInstance<ConstantLaserTE>().Kill(i, j);
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Rectangle frame = TileExtensions.BasicFrame(i, j) with { Y = 18 };
		spriteBatch.Draw(TextureAssets.Tile[Type].Value, TileExtensions.DrawPosition(i, j), frame, Color.White);
	}

	public class ConstantLaserProj : ModProjectile
	{
		private ref float Init => ref Projectile.ai[0];
		private ref float Length => ref Projectile.localAI[0];

		private bool CanBeBroken
		{
			get => Projectile.ai[1] == 1;
			set => Projectile.ai[1] = value ? 1 : 0;
		}

		private bool CurrentlyBroken
		{
			get => Projectile.localAI[1] == 1;
			set => Projectile.localAI[1] = value ? 1 : 0;
		}

		private ref float BreakLength => ref Projectile.ai[2];
		private ref float FlickerStartTime => ref Projectile.localAI[2];

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Projectile.timeLeft = 2;

			if (Init == 0)
			{
				Init = 1;
				Length = 0;

				Vector2 pos = Projectile.Center + Projectile.velocity * 36;

				while (!Collision.SolidCollision(pos, 8, 8))
				{
					pos += Projectile.velocity * 12;
				}

				Length = Projectile.Center.Distance(pos);
			}

			if (CanBeBroken)
			{
				BreakLength--;
				FlickerStartTime++;

				if (BreakLength <= 0)
				{
					CurrentlyBroken = !CurrentlyBroken;

					BreakLength = Main.rand.Next(180, 240);
					Projectile.netUpdate = true;

					if (!CurrentlyBroken)
					{
						FlickerStartTime = 0;
					}
				}
			}

			if (!CurrentlyBroken)
			{
				for (int i = 0; i < Length / 16; ++i)
				{
					Lighting.AddLight(Projectile.Center + Projectile.velocity * i * 16, TorchID.Red);
				}
			}
		}

		public override bool CanHitPlayer(Player target)
		{
			return !CurrentlyBroken && CanBeBroken && !(BreakLength < 60 && !CurrentlyBroken || FlickerStartTime < 60);
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			if (Projectile.velocity.X == 1)
			{
				hitbox.Location = new Point((int)Projectile.position.X, (int)Projectile.position.Y);
				hitbox.Width = (int)Length;
				hitbox.Height = 4;
			}
			else if (Projectile.velocity.X == -1)
			{
				hitbox.Location = new Point((int)(Projectile.position.X - Length), (int)Projectile.position.Y);
				hitbox.Width = (int)Length;
				hitbox.Height = 4;
			}
			else if (Projectile.velocity.Y == 1)
			{
				hitbox.Location = new Point((int)Projectile.position.X, (int)Projectile.position.Y);
				hitbox.Width = 4;
				hitbox.Height = (int)Length;
			}
			else if (Projectile.velocity.Y == -1)
			{
				hitbox.Location = new Point((int)Projectile.position.X, (int)(Projectile.position.Y - Length));
				hitbox.Width = 4;
				hitbox.Height = (int)Length;
			}
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float alpha = 1f;

			if (CanBeBroken)
			{
				if (CurrentlyBroken)
				{
					return false;
				}
				else if ((BreakLength < 60 && !CurrentlyBroken || FlickerStartTime < 60) && !Main.rand.NextBool(3))
				{
					if (Main.rand.NextBool(2))
					{
						return false;
					}
					else
					{
						alpha = 0.5f;
					}
				}
			}

			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			Main.spriteBatch.Draw(tex, pos, null, Color.White * alpha, Projectile.velocity.ToRotation(), new(0, 1), new Vector2(Length / 4, 1), SpriteEffects.None, 0);

			return false;
		}
	}
}

public class BrokenLaser : ConstantLaser
{
}

public class ConstantLaserTE : ModTileEntity
{
	public const int MaxTimer = 6 * 60;

	private bool _hasShot = false;

	public override bool IsTileValidForEntity(int x, int y)
	{
		return Main.tile[x, y].TileType == ModContent.TileType<ConstantLaser>() || Main.tile[x, y].TileType == ModContent.TileType<BrokenLaser>();
	}

	public override void Update()
	{
		if (!_hasShot)
		{
			Vector2 worldPos = Position.ToWorldCoordinates();
			Tile tile = Main.tile[Position];
			int side = tile.TileFrameX / 18;

			Vector2 direction = side switch
			{
				0 or 4 => new Vector2(0, -1),
				1 or 5 => new Vector2(0, 1),
				2 or 6 => new Vector2(1, 0),
				3 or _ => new Vector2(-1, 0)
			};

			Vector2 position = worldPos + direction * 10 + new Vector2(1, 0);
			var src = new EntitySource_TileUpdate(Position.X, Position.Y);
			int type = ModContent.ProjectileType<ConstantLaser.ConstantLaserProj>();
			bool broken = tile.TileType == ModContent.TileType<BrokenLaser>();
			int proj = Projectile.NewProjectile(src, position, direction, type, 40, 0, Main.myPlayer, 0, broken ? 1 : 0);

			_hasShot = true;
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

public class ConstantLaserItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<ConstantLaser>(), 0);
	}
}

public class BrokenLaserItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<BrokenLaser>(), 0);
	}
}
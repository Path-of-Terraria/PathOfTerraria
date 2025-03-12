using Microsoft.Xna.Framework.Graphics.PackedVector;
using PathOfTerraria.Common.Tiles;
using System.Diagnostics;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

public class ViceSpearAnchor : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.Origin = new Point16(0, 0);

		PincerTE tileEntity = ModContent.GetInstance<PincerTE>();
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
		ModContent.GetInstance<PincerTE>().Kill(i, j);
	}

	public class PincerProjectile : ModProjectile
	{
		private ref float State => ref Projectile.ai[0];
		private ref float Length => ref Projectile.ai[1];

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Projectile.timeLeft = 2;
			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

			if (State == 0)
			{
				State = 1;
				Length = 20;
			}
			else if (State == 1)
			{
				Player target = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];

				if (ExtensionCondition(target))
				{
					State = 2;
				}
			}
			else if (State == 2)
			{
				Length += 8;

				if (Collision.SolidCollision(Projectile.position + Projectile.velocity * Length, Projectile.width, Projectile.height))
				{
					State = 3;
				}
			}
			else if (State == 3)
			{
				Length -= 3;

				if (Length < 20)
				{
					State = 1;
				}
			}
		}

		private bool ExtensionCondition(Player target)
		{
			if (Projectile.velocity == new Vector2(0, 1))
			{
				return Math.Abs(target.Center.X - Projectile.Center.X) < 30 && Projectile.Center.Y < target.Center.Y;
			}
			else if (Projectile.velocity == new Vector2(0, -1))
			{
				return Math.Abs(target.Center.X - Projectile.Center.X) < 30 && Projectile.Center.Y > target.Center.Y;
			}
			else if (Projectile.velocity == new Vector2(1, 0))
			{
				return Math.Abs(target.Center.Y - Projectile.Center.Y) < 30 && Projectile.Center.X < target.Center.X;
			}
			else if (Projectile.velocity == new Vector2(-1, 0))
			{
				return Math.Abs(target.Center.Y - Projectile.Center.Y) < 30 && Projectile.Center.X > target.Center.X;
			}

			return false;
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			hitbox.Location = (hitbox.Location.ToVector2() + Projectile.velocity * Length).ToPoint();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;

			DrawChain(tex);

			Vector2 pos = Projectile.Center - Main.screenPosition + Projectile.velocity * Length;
			var frame = new Rectangle(State == 2 ? 36 : 10, 0, 24, 20);
			Color color = Lighting.GetColor((pos + Main.screenPosition).ToTileCoordinates());
			Main.spriteBatch.Draw(tex, pos, frame, color, Projectile.rotation, frame.Size() / 2f, Vector2.One, SpriteEffects.None, 0);
			Main.spriteBatch.Draw(tex, pos, frame with { Y = 22 }, Color.White, Projectile.rotation, frame.Size() / 2f, Vector2.One, SpriteEffects.None, 0);

			return false;
		}

		private void DrawChain(Texture2D tex)
		{
			for (int i = 0; i < Length / 6f; ++i)
			{
				Vector2 pos = Projectile.Center - Main.screenPosition + Projectile.velocity * i * 6;
				var frame = new Rectangle(0, 0, 8, 6);
				Color color = Lighting.GetColor((pos + Main.screenPosition).ToTileCoordinates());
				Main.spriteBatch.Draw(tex, pos, frame, color, Projectile.rotation, frame.Size() / 2f, Vector2.One, SpriteEffects.None, 0);
			}
		}
	}
}

public class PincerTE : ModTileEntity
{
	public const int MaxTimer = 6 * 60;

	private bool _hasShot = false;

	public override bool IsTileValidForEntity(int x, int y)
	{
		return Main.tile[x, y].TileType == ModContent.TileType<ViceSpearAnchor>();
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

			var src = new EntitySource_TileUpdate(Position.X, Position.Y);
			int type = ModContent.ProjectileType<ViceSpearAnchor.PincerProjectile>();
			int proj = Projectile.NewProjectile(src, worldPos, direction, type, 40, 0, Main.myPlayer, 0);

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

public class PincerAnchorItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<ViceSpearAnchor>(), 0);
	}
}

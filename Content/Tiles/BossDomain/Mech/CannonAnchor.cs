using PathOfTerraria.Common.NPCs;
using SubworldLibrary;
using System.IO;
using System.Linq;
using Terraria.Audio;
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

	public class ExplosiveTurret : ModNPC
	{
		public bool BombCannon
		{
			get => NPC.ai[0] == 1;
			set => NPC.ai[0] = value ? 1 : 0;
		}

		private ref float ShootTimer => ref NPC.ai[1];
		private ref float Blowback => ref NPC.ai[2];

		public override void SetDefaults()
		{
			NPC.noTileCollide = true;
			NPC.timeLeft = 6;
			NPC.aiStyle = -1;
			NPC.width = 6;
			NPC.height = 6;
			NPC.lifeMax = 1500;
			NPC.dontTakeDamage = true;
			NPC.dontCountMe = true;
			NPC.defense = 20000;
			NPC.noGravity = true;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void AI()
		{
			if (Main.CurrentFrameFlags.ActivePlayersCount == 0)
			{
				return;
			}

			int targetWho = Player.FindClosest(NPC.position, NPC.width, NPC.height);
			Player target = Main.player[targetWho];

			if (!target.active || target.dead || target.DistanceSQ(NPC.Center) > 6000 * 6000)
			{
				return;
			}

			NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.AngleTo(target.Center + target.velocity * 20) - MathHelper.PiOver2, 0.12f);

			Blowback = MathHelper.Lerp(Blowback, 0, 0.07f);
			ShootTimer++;

			if (ShootTimer > 60)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient && SubworldSystem.Current is not null)
				{
					int type = BombCannon ? ModContent.ProjectileType<PrimeBomb>() : ModContent.ProjectileType<PrimeRocket>();
					Vector2 vel = (NPC.rotation + MathHelper.PiOver2).ToRotationVector2() * 8;
					int damage = BombCannon ? ModeUtils.ProjectileDamage(120, 170, 200) : ModeUtils.ProjectileDamage(70, 100, 160);
					int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + vel * 2, vel, type, damage, 0, Main.myPlayer);

					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
					}
				}

				Blowback = 8;
				ShootTimer = 0;
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(NPC.rotation);
			writer.Write(ShootTimer);
			writer.Write(BombCannon);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			NPC.rotation = reader.ReadSingle();
			ShootTimer = reader.ReadSingle();
			BombCannon = reader.ReadBoolean();
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = TextureAssets.Npc[Type].Value;
			Rectangle frame = new(0, 0, 22, 22);
			Main.spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, frame, drawColor, NPC.rotation, frame.Size() / 2f, 1f, SpriteEffects.None, 0);
			
			frame = new(BombCannon ? 28 : 0, 26, 26, 44);
			Main.spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, frame, drawColor, NPC.rotation, new Vector2(13, -6 + Blowback), 1f, SpriteEffects.None, 0);
			return false;
		}
	}

	public class PrimeRocket : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.Explosive[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.RocketII);
			Projectile.width = Projectile.height = 10;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.netImportant = true;

			AIType = ProjectileID.RocketII;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.Kill();
			return true;
		}

		public override void OnKill(int timeLeft)
		{
			const int Range = 4;

			for (int i = 0; i < 25; ++i)
			{
				Vector2 vel = Main.rand.NextVector2CircularEdge(3, 3) * Main.rand.NextFloat(0.2f, 1f);
				Dust.NewDustPerfect(Projectile.position, DustID.Torch, vel, 0, default, Main.rand.NextFloat(1.5f, 3));
			}

			for (int i = 0; i < 6; ++i)
			{
				Vector2 vel = Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(0.2f, 1f);
				Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, vel, GoreID.Smoke1 + Main.rand.Next(3));
			}

			SoundEngine.PlaySound(SoundID.Item14 with { MaxInstances = 0 }, Projectile.Center);

			Projectile.Resize(120, 120);
			Projectile.Opacity = 0f;
			Projectile.Damage();
			Projectile.tileCollide = false;

			Point16 center = Projectile.Center.ToTileCoordinates16();

			for (int i = -Range; i < Range + 1; ++i)
			{
				for (int j = -Range; j < Range + 1; ++j)
				{
					if (new Vector2(i, j).Length() < Range)
					{
						Point16 pos = new(center.X + i, center.Y + j);

						if (TileLoader.CanExplode(center.X + i, center.Y + j))
						{
							WorldGen.KillTile(pos.X, pos.Y);
						}
					}
				}
			}

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendTileSquare(-1, center.X - Range, center.Y - Range, Range * 2, Range * 2);
			}
		}

		public override bool CanHitPlayer(Player target)
		{
			return Projectile.Opacity == 0;
		}
	}

	public class PrimeBomb : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.Explosive[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.RocketII);
			Projectile.width = Projectile.height = 24;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = -1;
			Projectile.netImportant = true;
		}

		public override void PostAI()
		{
			Projectile.rotation += Projectile.velocity.X * 0.05f;
			Projectile.velocity.Y += 0.2f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
			{
				Projectile.velocity.X = -oldVelocity.X;
			}

			if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
			{
				Projectile.velocity.Y = -oldVelocity.Y * 0.3f;
			}

			Projectile.velocity *= 0.99f;
			return false;
		}

		public override bool PreKill(int timeLeft)
		{
			Explode();
			return false;
		}

		private void Explode()
		{
			const int Range = 6;

			for (int i = 0; i < 25; ++i)
			{
				Vector2 vel = Main.rand.NextVector2CircularEdge(3, 3) * Main.rand.NextFloat(0.2f, 1f);
				Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0, default, Main.rand.NextFloat(1.5f, 3));
			}

			for (int i = 0; i < 6; ++i)
			{
				Vector2 vel = Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(0.2f, 1f);
				Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
			}

			SoundEngine.PlaySound(SoundID.Item14 with { MaxInstances = 0 }, Projectile.Center);

			Projectile.Resize(150, 150);
			Projectile.Opacity = 0f;
			Projectile.Damage();

			Point16 center = Projectile.Center.ToTileCoordinates16();

			for (int i = -Range; i < Range + 1; ++i)
			{
				for (int j = -Range; j < Range + 1; ++j)
				{
					if (new Vector2(i, j).Length() < Range)
					{
						Point16 pos = new(center.X + i, center.Y + j);

						if (TileLoader.CanExplode(center.X + i, center.Y + j))
						{
							WorldGen.KillTile(pos.X, pos.Y);
						}
					}
				}
			}

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendTileSquare(-1, center.X - Range, center.Y - Range, Range * 2, Range * 2);
			}
		}

		public override bool CanHitPlayer(Player target)
		{
			return Projectile.Opacity == 0;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			var source = new Rectangle(0, 0, 24, 24);
			Vector2 position = Projectile.Center - Main.screenPosition;

			Main.EntitySpriteDraw(tex, position, source, lightColor, Projectile.rotation, source.Size() / 2f, 1f, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(tex, position, source with { Y = 26 }, Color.White, Projectile.rotation, source.Size() / 2f, 1f, SpriteEffects.None, 0);
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

			int type = ModContent.NPCType<CannonAnchor.ExplosiveTurret>();
			Tile tile = Main.tile[Position];
			int bomb = tile.TileType == ModContent.TileType<CannonBombAnchor>() ? 1 : 0;
			int npc = NPC.NewNPC(new EntitySource_TileEntity(this), Position.X * 16, Position.Y * 16, type, 0, bomb, Main.rand.NextFloat(120));
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

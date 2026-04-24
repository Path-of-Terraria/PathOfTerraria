using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp;

internal class BloatingLeech : Gear
{
	internal class BloatingLeechProjectile : ModProjectile
	{
		private readonly static ExplosionHitbox.VFXPackage ExplosionVFX = new(4, 20, 10, true, 0.8f, null, DustID.Blood, DustID.BloodWater, 4f, true);

		private Player Owner => Main.player[Projectile.owner];
		private ref float State => ref Projectile.ai[0];
		private ref float RetractionTime => ref Projectile.ai[1];
		private ref float AttachedNPC => ref Projectile.ai[2];

		private Vector2 Offset
		{
			get => new(Projectile.localAI[0], Projectile.localAI[1]);
			set => (Projectile.localAI[0], Projectile.localAI[1]) = (value.X, value.Y);
		}

		private ref float HitsDone => ref Projectile.localAI[2];

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 5;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.Size = new(14, 14);
			Projectile.friendly = true;
			Projectile.timeLeft = 2;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Projectile.timeLeft++;
			Owner.SetDummyItemTime(2);

			if (State == 0)
			{
				Projectile.velocity.Y += 0.1f;

				if (Projectile.DistanceSQ(Owner.Center) >= 700 * 700 || (Main.myPlayer == Projectile.owner && Main.mouseRight))
				{
					State = 1;
				}
			}
			else if (State == 1)
			{
				RetractionTime = MathHelper.Min(RetractionTime + 1, 12 * 60);
				Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, Projectile.DirectionTo(Owner.Center) * 14, 0.1f + (RetractionTime / 60f));

				if (Projectile.DistanceSQ(Owner.Center) <= 10 * 10)
				{
					Projectile.Kill();
				}
			}
			else if (State == 2)
			{
				NPC npc = Main.npc[(int)AttachedNPC];
				Projectile.Center = npc.Center + Offset;

				if (!npc.active || HitsDone >= 20 || Owner.DistanceSQ(Projectile.Center) > 800 * 800)
				{
					Explode();
					return;
				}

				if (Main.myPlayer == Projectile.owner && Main.mouseRight)
				{
					Explode();
					Projectile.Kill();
				}
			}
		}

		private void Explode()
		{
			Vector2 size = new Vector2(400) * (HitsDone / 20f);
			float damageModifier = (HitsDone / 20f) * 5f;
			int damage = (int)(Projectile.damage * damageModifier);

			ExplosionHitbox.QuickSpawn(Projectile.GetSource_Death(), Projectile, damage, Projectile.owner, size, ExplosionSpawnInfo.FriendlySpawn, ExplosionVFX with 
			{ 
				DustVelocityModifier = 0.5f + HitsDone / 20f * 4f
			});
			Projectile.Kill();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			HitsDone++;

			if (target.TryGetGlobalNPC(out BleedDebuffNPC bleed) && bleed.Stacks.Length > 0)
			{
				HitsDone += 2;
			}

			if (State == 2)
			{
				return;
			}

			Projectile.velocity = Vector2.Zero;
			Offset = Projectile.Center - target.Center;
			State = 2;
			AttachedNPC = target.whoAmI;
		}

		public override void OnKill(int timeLeft)
		{
			Vector2 dir = Projectile.DirectionTo(Owner.Center);
			float distance = Projectile.Distance(Owner.Center);

			for (int i = 0; i < distance / 8f; ++i)
			{
				Vector2 offset = dir * i * 8;

				Dust.NewDust(Projectile.position + offset, Projectile.width, Projectile.height, DustID.Blood);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Vector2 pos = Projectile.Center;
			float distance = Projectile.Distance(Owner.Center);
			Vector2 dir = (Owner.Center - Projectile.Center) / distance;
			float rotation = dir.ToRotation();
			float baseScale = HitsDone / 10f + 1;
			float originalScale = baseScale;
			float shakeFactor = distance > 500f ? (distance - 500f) / 300f : 0;
			var stretchColor = Color.Lerp(Color.White, Color.Red, shakeFactor);

			for (int i = 0; i < 8; ++i)
			{
				int frame = i switch
				{
					0 => 64,
					1 or 2 => 48,
					2 or 3 => 32,
					4 or 5 or 6 => 16,
					_ => 0,
				};

				Rectangle src = new(frame == 64 && Main.GameUpdateCount % 30 < 15 ? 16 : 0, frame, 14, 14);
				Vector2 offset = dir * i * (distance / 8f);
				Vector2 scale = new(baseScale, distance / 8f / 14f);
				float factor = 0;

				if (i < 5)
				{
					baseScale = MathHelper.Lerp(originalScale, 1, i / 4f);
					factor = 1 - i / 4f;
				}

				Point16 center = (Projectile.Center + offset).ToTileCoordinates16();
				Color color = Lighting.GetColor(center.X, center.Y, Color.Lerp(stretchColor, Color.Red, factor * MathF.Min(HitsDone / 20f, 1)));
				Vector2 position = pos + offset - Main.screenPosition;
				float drawRotation = rotation - MathHelper.PiOver2 + MathF.Sin(Main.GameUpdateCount * 0.2f + i) * 0.025f;

				Main.spriteBatch.Draw(tex, position, src, color, drawRotation, new Vector2(7, 0), scale, SpriteEffects.FlipVertically, 0);
			}

			return false;
		}
	}

	protected override string GearLocalizationCategory => "Summon";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
		staticData.Description = this.GetLocalization("Description");
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
	}

	public override void SetDefaults()
	{
		Item.DamageType = DamageClass.Summon;
		Item.damage = 150;
		Item.useTime = 20;
		Item.useAnimation = 20;
		Item.channel = true;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.shoot = ModContent.ProjectileType<BloatingLeechProjectile>();
		Item.shootSpeed = 9;
		Item.noUseGraphic = true;
		Item.noMelee = true;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Summon;
	}
}

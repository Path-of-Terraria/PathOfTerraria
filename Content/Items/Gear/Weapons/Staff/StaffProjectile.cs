using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

/// <summary>
/// Charges up, then releases. The actual projectile for a staff.
/// </summary>
internal abstract class StaffProjectile : ModProjectile
{
	public virtual int DustType => DustID.GemAmethyst;
	public virtual int TorchType => TorchID.Purple;
	public virtual int MaxCharge => 60;
	public virtual Vector2 ChargeOffset => new(70);

	protected Player Owner => Main.player[Projectile.owner];

	private ref float Charge => ref Projectile.ai[0];

	private bool LetGo
	{
		get => Projectile.ai[1] == 1;
		set => Projectile.ai[1] = value ? 1 : 0;
	}

	private bool PassedCharge
	{
		get => Projectile.ai[2] == 1;
		set => Projectile.ai[2] = value ? 1 : 0;
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Type] = 5;
		ProjectileID.Sets.TrailingMode[Type] = 0;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new(16);
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.timeLeft = 3000;
		Projectile.tileCollide = false;
	}

	public override bool? CanDamage()
	{
		return LetGo;
	}

	public override bool ShouldUpdatePosition()
	{
		return LetGo;
	}

	public override void AI()
	{
		Projectile.rotation += 0.1f;

		TorchID.TorchColor(TorchType, out float r, out float g, out float b);
		Lighting.AddLight(Projectile.Center, new Vector3(r, g, b) * 0.25f);

		if (Main.rand.NextBool((int)MathHelper.Lerp(30, 16, Projectile.scale)))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType);
		}

		if (LetGo)
		{
			if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
			{
				Projectile.tileCollide = true;
			}

			return;
		}

		if (!Owner.channel && Charge < MaxCharge)
		{
			Projectile.Kill();
			return;
		}

		if (Owner.GetModPlayer<StaffPlayer>().Empowered)
		{
			Charge += 7;
		}

		if (++Charge < MaxCharge || Owner.channel)
		{
			Projectile.timeLeft++;
			Projectile.scale = Math.Min(1, Charge / MaxCharge);

			if (Main.myPlayer == Projectile.owner)
			{
				Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center + Projectile.DirectionTo(Main.MouseWorld) * ChargeOffset, 0.2f);

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
				}
			}

			Projectile.Opacity = Charge / MaxCharge;
		}
		else if (!Owner.channel && !LetGo)
		{
			ReleaseProjectile();
		}

		if (!PassedCharge && Charge >= MaxCharge)
		{
			if (Owner.GetModPlayer<StaffPlayer>().Empowered) // Automatically release projectile for easier spam
			{
				ReleaseProjectile();
			}
			else // Don't play sound when empowered, it's annoying
			{
				SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
			}

			for (int i = 0; i < 4; ++i)
			{
				Vector2 vel = Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(1f, 1.2f);
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType, vel.X, vel.Y);
			}

			PassedCharge = true;
		}
	}

	public void ReleaseProjectile()
	{
		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * Owner.HeldItem.shootSpeed;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
			}
		}

		LetGo = true;

		if (!Owner.CheckMana(Owner.HeldItem.mana, true))
		{
			Projectile.Kill();
			Owner.channel = false;
			return;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition;

		for (int k = 0; k < Projectile.oldPos.Length; k++)
		{
			Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + tex.Size() / 2f + new Vector2(0f, Projectile.gfxOffY);
			Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
			DrawSelf(color, tex, drawPos);
		}

		DrawSelf(lightColor, tex, position);

		return false;
	}

	private void DrawSelf(Color lightColor, Texture2D tex, Vector2 position)
	{
		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
			float scale = 1.2f - i * 0.3f;
			Color color = Projectile.GetAlpha(lightColor) * (0.3f + i * 0.3f) * Projectile.Opacity;
			Main.EntitySpriteDraw(tex, position, null, color, rotation, tex.Size() / 2f, scale * Projectile.scale, SpriteEffects.None, 0);
		}
	}

	public override void OnKill(int timeLeft)
	{
		int repeats = (int)(15 * Projectile.scale);

		for (int i = 0; i < repeats; ++i)
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f);
		}
	}
}

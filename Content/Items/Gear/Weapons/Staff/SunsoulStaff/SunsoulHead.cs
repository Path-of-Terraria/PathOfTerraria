using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff.SunsoulStaff;

internal class SunsoulHead : StaffProjectile
{
	public override int DustType => DustID.AncientLight;
	public override int MaxCharge => 90;
	public override int TorchType => TorchID.Yellow;
	public override bool CanCollideWithTiles => false;

	private int _nextHead = 0;
	private float _headTimer = 0;

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.penetrate = -1;
		Projectile.Size = new Vector2(62);
		Projectile.localNPCHitCooldown = 20;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.tileCollide = false;
	}

	public override void AI()
	{
		base.AI();
		SetRotation();

		if (LetGo && (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height) || Projectile.Opacity < 1f))
		{
			Projectile.Opacity *= 0.92f;
			Projectile.velocity *= 0.92f;

			if (Projectile.Opacity < 0.1f)
			{
				Projectile.Kill();
				return;
			}
		}

		if (LetGo && _nextHead < 3 && ++_headTimer > 5)
		{
			_nextHead++;
			_headTimer = 0;

			Vector2 vel = Projectile.velocity.RotatedByRandom(0.2f);
			int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Owner.Center - new Vector2(0, 60), vel, Type, Projectile.damage, 0, Projectile.owner);
			Main.projectile[proj].scale = Main.rand.NextFloat(0.8f, 0.95f);
			var head = Main.projectile[proj].ModProjectile as SunsoulHead;
			head._nextHead = 5;
			head._headTimer = Main.rand.NextFloat(-0.1f, 0.1f);
			head.Charge = MaxCharge;
			head.LetGo = true;
		}
	}

	private void SetRotation()
	{
		Projectile.rotation = LetGo ? Projectile.velocity.ToRotation() : Main.myPlayer == Projectile.owner ? Projectile.AngleTo(Main.MouseWorld) : 0;
		Projectile.spriteDirection = -1;

		if (_nextHead == 5)
		{
			Projectile.rotation += _headTimer;
		}

		if (Math.Abs(Projectile.rotation) > MathHelper.PiOver2)
		{
			Projectile.rotation += MathHelper.Pi;
			Projectile.spriteDirection = 1;
		}
	}

	public override void ReleaseProjectile()
	{
		if (Main.myPlayer == Projectile.owner)
		{
			Vector2 vel = Projectile.DirectionTo(Main.MouseWorld);

			if (_nextHead == 5)
			{
				vel = vel.RotatedBy(_headTimer);
			}

			Projectile.velocity = vel * Owner.HeldItem.shootSpeed;

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
		}
	}

	public override void OnKill(int timeLeft)
	{
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		lightColor = Color.White;

		for (int k = 0; k < Projectile.oldPos.Length; k++)
		{
			Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + tex.Size() / 2f + new Vector2(0f, Projectile.gfxOffY);
			Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
			DrawSelf(color, tex, drawPos);
		}

		Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY);
		DrawSelf(lightColor, tex, position);

		return false;
	}

	private void DrawSelf(Color lightColor, Texture2D tex, Vector2 position)
	{
		SpriteEffects effect = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation;
			float scale = 1.2f - i * 0.3f;
			Color color = Projectile.GetAlpha(lightColor) * (0.1f + i * 0.3f) * Projectile.Opacity;
			Main.EntitySpriteDraw(tex, position, null, color, rotation, tex.Size() / 2f, scale * Projectile.scale, effect, 0);
		}
	}
}

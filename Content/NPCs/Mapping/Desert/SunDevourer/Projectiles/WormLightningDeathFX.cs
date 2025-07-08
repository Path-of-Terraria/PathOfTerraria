using System.IO;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;

internal class WormLightningDeathFX : ModProjectile
{
	public ref float NpcType => ref Projectile.ai[0];
	public ref float Timer => ref Projectile.ai[1];
	public ref float Rotation => ref Projectile.ai[2];
	public ref float MaxTimer => ref Projectile.localAI[0];

	private NPC _drawDummy = null;

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 120;
		Projectile.Size = new Vector2(24);
		Projectile.tileCollide = false;
	}

	public override void AI()
	{
		Timer++;

		if (MaxTimer == 0)
		{
			MaxTimer = Main.rand.Next(30, 180);
			Projectile.netUpdate = true;
		}

		Projectile.Opacity = 1 - Timer / MaxTimer;
		Projectile.velocity *= 0.97f;
		Projectile.rotation = Rotation;

		if (Timer > MaxTimer / 2)
		{
			Projectile.Kill();

			for (int i = 0; i < 8; ++i)
			{
				var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
				dust.velocity *= 2f;
				dust.noGravity = true;
			}

			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/CrunchyPop") with { PitchRange = (-0.7f, 0.3f), Volume = 0.2f }, Projectile.Center);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (_drawDummy is null)
		{
			_drawDummy = new NPC();
			_drawDummy.SetDefaults((int)NpcType);
		}

		_drawDummy.Center = Projectile.Center;
		_drawDummy.Opacity = Projectile.Opacity;
		_drawDummy.rotation = Projectile.rotation;
		WormLightning.DrawSelf(_drawDummy, Main.spriteBatch, Main.screenPosition);
		return false;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write((Half)MaxTimer);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		MaxTimer = (float)reader.ReadHalf();
	}
}

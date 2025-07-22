using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class ExplosionHitbox : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_0";

	private ref float Width => ref Projectile.ai[0];
	private ref float Height => ref Projectile.ai[1];

	public override void SetDefaults()
	{
		Projectile.friendly = true;
		Projectile.hostile = true;
		Projectile.timeLeft = 3;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(100, 100);
		Projectile.Opacity = 0.5f;
		Projectile.netImportant = true;
		Projectile.penetrate = -1;
	}

	public override void AI()
	{
		if (Projectile.width != (int)Width || Projectile.height != (int)Height)
		{
			Projectile.Resize((int)Width, (int)Height);
		}
	}

	/// <summary>
	/// Copies vanilla's bomb/grenade explosion VFX with some modifyability.
	/// </summary>
	/// <param name="entity">Enity that's spawning the VFX.</param>
	/// <param name="goreRepeats">How many times to <b>repeat</b> the gore spawning. Spawns 4 gore per repeat. Defaults to 1 repeat.</param>
	/// <param name="smokeDustCount">How many smoke dusts to spawn. Defaults to 20.</param>
	/// <param name="torchDustCount">
	/// How many torch dusts to spawn. Defaults to 10. 
	/// Note that this will loop <paramref name="torchDustCount"/> / 2 times, since the loop has two dust spawns in it..
	/// </param>
	/// <param name="sfx">Whether to play the SFX or not.</param>
	public static void VFX(Entity entity, int goreRepeats = 1, int smokeDustCount = 20, int torchDustCount = 10, bool sfx = true)
	{
		if (sfx && !Main.dedServ)
		{
			SoundEngine.PlaySound(in SoundID.Item14, entity.position);
		}

		for (int i = 0; i < smokeDustCount; i++)
		{
			int dust = Dust.NewDust(entity.position, entity.width, entity.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
			Dust newDust = Main.dust[dust];
			newDust.velocity *= 1.4f;
		}

		const int TorchId = DustID.Torch;

		for (int i = 0; i < torchDustCount / 2; i++)
		{
			int dust = Dust.NewDust(entity.position, entity.width, entity.height, TorchId, 0f, 0f, 100, default, 2.5f);
			Dust newDust = Main.dust[dust];
			newDust.noGravity = true;
			newDust.velocity *= 5f;

			dust = Dust.NewDust(entity.position, entity.width, entity.height, TorchId, 0f, 0f, 100, default, 1.5f);
			newDust = Main.dust[dust];
			newDust.velocity *= 3f;
		}

		if (Main.dedServ)
		{
			return;
		}

		IEntitySource src = entity.GetSource_Death();

		for (int i = 0; i < goreRepeats; ++i)
		{
			int slot = Gore.NewGore(src, entity.position, default, RandomSmoke());
			Gore gore = Main.gore[slot];
			gore.velocity *= 0.4f;
			gore.velocity.X += 1f;
			gore.velocity.Y += 1f;

			slot = Gore.NewGore(src, entity.position, default, RandomSmoke());
			gore = Main.gore[slot];
			gore.velocity *= 0.4f;
			gore.velocity.X -= 1f;
			gore.velocity.Y += 1f;

			slot = Gore.NewGore(src, entity.position, default, RandomSmoke());
			gore = Main.gore[slot];
			gore.velocity *= 0.4f;
			gore.velocity.X += 1f;
			gore.velocity.Y -= 1f;

			slot = Gore.NewGore(src, entity.position, default, RandomSmoke());
			gore = Main.gore[slot];
			gore.velocity *= 0.4f;
			gore.velocity.X -= 1f;
			gore.velocity.Y -= 1f;
		}

		return;

		static int RandomSmoke()
		{
			return Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1);
		}
	}
}

internal class ExplosionHitboxFriendly : ExplosionHitbox
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.hostile = false;
	}
}
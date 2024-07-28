using PathOfTerraria.Common.Utilities;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Ranged;

internal class BoomerangProjectile : ModProjectile
{
	public override string Texture => $"Terraria/Images/NPC_0"; // Use an empty texture since, by default, this uses the item's texture

	private Player Owner => Main.player[Projectile.owner];

	private bool Orbit
	{
		get => Projectile.ai[0] == 1;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	private ref float ItemId => ref Projectile.ai[1];
	private ref float Timer => ref Projectile.ai[2];

	private float _originalMagnitude = 0;
	private sbyte _originalDirection = 0;

	public override void SetDefaults()
	{
		Projectile.width = 20;
		Projectile.height = 20;
		Projectile.tileCollide = false;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
	}

	public override void AI()
	{
		Timer++;
		Projectile.rotation += 0.25f;

		if (Timer == 1)
		{
			_originalMagnitude = Projectile.velocity.Length(); // Store original velocity magnitude to keep speed consistent
			_originalDirection = (sbyte)Owner.direction;

			Vector2 size = ContentSamples.ItemsByType[(int)ItemId].Size;
			int edge = (int)Math.Min(size.X, size.Y);
			Projectile.width = Projectile.height = edge; // Adjust hitbox to be a square
		}

		if (Orbit)
		{
			if (Timer < 100)
			{
				// Create a point that spins around the player with a variable length
				Vector2 relativePosition = Owner.Center + new Vector2(0, (50 - Math.Abs(Timer - 50)) * 5).RotatedBy(Timer / 8f * _originalDirection);

				// Give the projectile a subtle version of that for velocity for hit effect's sake, and lerp it towards the point
				Projectile.velocity = Projectile.DirectionTo(relativePosition);
				Projectile.Center = Vector2.Lerp(Projectile.Center, relativePosition, 0.2f);
			}
			else // Projectile will usually be touching the player by now, unless the player is fast - simply return to the player otherwise
			{
				Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center, 0.2f);
				ReturnToPlayer();
				Projectile.velocity *= 0.1f;
			}
		}
		else
		{
			if (Timer > 30)
			{
				ReturnToPlayer();
			}
		}
	}

	private void ReturnToPlayer()
	{
		float factor = 0.05f;

		if (Timer > 690) // Make the boomerang return more and more aggressively if out in the air for too long
		{
			_originalMagnitude += 0.1f;
			factor += Math.Min((Timer - 90f) / 120f, 1);
		}

		// Return by lerping velocity towards player.
		Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Owner.Center) * _originalMagnitude, factor);

		if (Owner.Hitbox.Intersects(Projectile.Hitbox)) // Despawn when touching the player.
		{
			Projectile.Kill();
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (Timer <= 30) // Only reflect & return if not already returning for vanilla parity
		{
			Projectile.velocity = -Projectile.velocity.RotatedByRandom(0.2f); // Reflect boomerang...
			Timer = 31; //...and make it return immediately
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureUtils.LoadAndGetItem((int)ItemId).Value;

		Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, 1, SpriteEffects.None, 0);
		return false;
	}
}

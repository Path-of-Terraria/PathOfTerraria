using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;

public sealed class SunBlast : ModProjectile
{
	public NPC Parent => Main.npc[(int)ParentWho];

	public ref float Timer => ref Projectile.ai[0];
	public ref float ParentWho => ref Projectile.ai[1];
	public ref float LifeTime => ref Projectile.ai[2];

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.DrawScreenCheckFluff[Type] = 8000;
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.tileCollide = false;

		Projectile.width = 80;
		Projectile.height = 80;
		Projectile.timeLeft = 4;
		Projectile.hide = true;
	}

	public override void DrawBehind(int index, List<int> bTnN, List<int> bN, List<int> bP, List<int> overPlayers, List<int> oWUI)
	{
		overPlayers.Add(index);
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		return false;
	}

	public override void AI()
	{
		if (!Parent.active)
		{
			Projectile.Kill();
			return;
		}

		Timer++;

		Vector2 toIdle = Parent.DirectionTo((Parent.ModNPC as SunDevourerNPC).IdleSpot);

		Projectile.Center = Parent.Center + toIdle * 170 + new Vector2(0, 10);
		Projectile.velocity = toIdle;
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		Projectile.timeLeft = 4;

		if (Timer > LifeTime)
		{
			Projectile.Kill();
		}

		if (Timer < LifeTime - 30)
		{
			Projectile.scale = MathF.Min(1, Timer / 120f);
		}
		else
		{
			float time = Timer - (LifeTime - 30);
			Projectile.scale = 1 - time / 30f;
		}

		Projectile.Opacity = Projectile.scale;
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		Vector2 end = Projectile.Center + Projectile.velocity * 15000;
		float point = 0;
		return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center, end, 72, ref point);
	}

	public override bool CanHitPlayer(Player target)
	{
		return Timer > 120 && Timer < LifeTime - 30;
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);
		Rectangle frame = new(0, 72, 190, 44);
		Vector2 origin = frame.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);
		Color color = Color.White;
		SpriteEffects effects = SpriteEffects.None;
		var scale = new Vector2(Projectile.scale, 1f);

		DrawSegment(texture, position, frame, origin, color, effects, scale);

		frame.Y = 0;
		frame.Height = 70;
		Vector2 dir = Vector2.Normalize(Projectile.velocity) * 70;
		float dist = 0f;
		int count = 0;

		while (Vector2.DistanceSquared(position + Main.screenPosition, Projectile.Center) < 6000 * 6000)
		{
			frame.X = 192 * (int)(Main.timeForVisualEffects * 0.4f % 3);
			position += dir;
			Vector2 drawPos = position + dir.RotatedBy(MathHelper.PiOver2) * MathF.Sin((float)Main.timeForVisualEffects * 0.2f + count * 0.4f) * 0.1f * dist;
			DrawSegment(texture, drawPos, frame, origin, color, effects, scale);
			dist = MathF.Min(1, dist + 0.34f);
			count++;

			Lighting.AddLight(position + Main.screenPosition, new Vector3(1.4f, 1.4f, 1.1f) * Projectile.scale);
		}

		return false;
	}

	private void DrawSegment(Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, Color color, SpriteEffects effects, Vector2 scale)
	{
		Color drawColor = Projectile.GetAlpha(color) * Projectile.Opacity;
		Main.EntitySpriteDraw(texture, position, frame, drawColor * 0.25f, Projectile.rotation, origin, scale * new Vector2(1.5f, 1), effects);
		Main.EntitySpriteDraw(texture, position, frame, drawColor * 0.55f, Projectile.rotation, origin, scale * new Vector2(1.2f, 1), effects);
		Main.EntitySpriteDraw(texture, position, frame, drawColor, Projectile.rotation, origin, scale, effects);
	}
}
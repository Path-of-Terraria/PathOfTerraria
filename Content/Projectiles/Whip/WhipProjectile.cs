using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Whip;

internal abstract class WhipProjectile : ModProjectile
{
	public float Timer
	{
		get => Projectile.ai[0];
		set => Projectile.ai[0] = value;
	}

	public float ChargeTime
	{
		get => Projectile.ai[1];
		set => Projectile.ai[1] = value;
	}

	public bool AltUse
	{
		get => Projectile.ai[2] == 1;
		set => Projectile.ai[2] = value ? 1 : 0;
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.IsAWhip[Type] = true;
	}

	public override void SetDefaults()
	{
		Projectile.DefaultToWhip();
	}

	// This example uses PreAI to implement a charging mechanic.
	// If you remove this, also remove Item.channel = true from the item's SetDefaults.
	public override bool PreAI()
	{
		Player owner = Main.player[Projectile.owner];

		if (AltUse || !owner.channel || ChargeTime >= 120)
		{
			return true; // Let the vanilla whip AI run.
		}

		if (++ChargeTime % 12 == 0) // 1 segment per 12 ticks of charge.
		{
			Projectile.WhipSettings.Segments++;
		}

		// Increase range up to 2x for full charge.
		Projectile.WhipSettings.RangeMultiplier += 1 / 120f;

		// Reset the animation and item timer while charging.
		owner.itemAnimation = owner.itemAnimationMax;
		owner.itemTime = owner.itemTimeMax;

		return false; // Prevent the vanilla whip AI from running.
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
		Projectile.damage = (int)(Projectile.damage * 0.5f); // Multihit penalty. Decrease the damage the more enemies the whip hits.
	}

	// This method draws a line between all points of the whip, in case there's empty space between the sprites.
	private static void DrawLine(List<Vector2> list)
	{
		Texture2D texture = TextureAssets.FishingLine.Value;
		Rectangle frame = texture.Frame();
		var origin = new Vector2(frame.Width / 2, 2);

		Vector2 pos = list[0];
		for (int i = 0; i < list.Count - 1; i++)
		{
			Vector2 element = list[i];
			Vector2 diff = list[i + 1] - element;

			float rotation = diff.ToRotation() - MathHelper.PiOver2;
			Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
			var scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

			Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

			pos += diff;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		var list = new List<Vector2>();
		Projectile.FillWhipControlPoints(Projectile, list);

		DrawLine(list);

		Main.DrawWhip_WhipBland(Projectile, list);
		// The code below is for custom drawing.
		// If you don't want that, you can remove it all and instead call one of vanilla's DrawWhip methods, like above.
		// However, you must adhere to how they draw if you do.

		//SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		//Texture2D texture = TextureAssets.Projectile[Type].Value;

		//Vector2 pos = list[0];

		//for (int i = 0; i < list.Count - 1; i++)
		//{
		//	// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
		//	// You can change them if they don't!
		//	var frame = new Rectangle(0, 0, 10, 26); // The size of the Handle (measured in pixels)
		//	var origin = new Vector2(5, 8); // Offset for where the player's hand will start measured from the top left of the image.
		//	float scale = 1;

		//	// These statements determine what part of the spritesheet to draw for the current segment.
		//	// They can also be changed to suit your sprite.
		//	if (i == list.Count - 2)
		//	{
		//		// This is the head of the whip. You need to measure the sprite to figure out these values.
		//		frame.Y = 74; // Distance from the top of the sprite to the start of the frame.
		//		frame.Height = 18; // Height of the frame.

		//		// For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
		//		Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
		//		float t = Timer / timeToFlyOut;
		//		scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
		//	}
		//	else
		//	{
		//		switch (i)
		//		{
		//			case > 10:
		//				// Third segment
		//				frame.Y = 58;
		//				frame.Height = 16;
		//				break;
		//			case > 5:
		//				// Second Segment
		//				frame.Y = 42;
		//				frame.Height = 16;
		//				break;
		//			case > 0:
		//				// First Segment
		//				frame.Y = 26;
		//				frame.Height = 16;
		//				break;
		//		}
		//	}

		//	Vector2 element = list[i];
		//	Vector2 diff = list[i + 1] - element;

		//	float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
		//	Color color = Lighting.GetColor(element.ToTileCoordinates());

		//	Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

		//	pos += diff;
		//}

		return false;
	}
}
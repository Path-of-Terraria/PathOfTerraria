using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Whip
{
	internal abstract class WhipProjectile : ModProjectile
	{
		private Player Owner => Main.player[Projectile.owner];

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

		public override void AI()
		{
			if (!AltUse)
			{
				return;
			}

			Projectile.frameCounter++;

			Player.CompositeArmStretchAmount stretch = Projectile.frameCounter switch
			{
				<= 10 => Player.CompositeArmStretchAmount.ThreeQuarters,
				<= 20 => Player.CompositeArmStretchAmount.Quarter,
				<= 30 => Player.CompositeArmStretchAmount.Full,
				_ => Player.CompositeArmStretchAmount.Full
			};

			float rotationOffset = Projectile.frameCounter switch
			{
				>= 50 => MathHelper.Pi,
				>= 40 => 3 * MathHelper.PiOver4,
				>= 30 => MathHelper.PiOver2,
				_ => -MathHelper.PiOver2
			};

			Owner.SetCompositeArmFront(true, stretch, rotationOffset);
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
			Projectile.damage =
				(int)(Projectile.damage *
				      0.5f); // Multihit penalty. Decrease the damage the more enemies the whip hits.
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

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale,
					SpriteEffects.None, 0);

				pos += diff;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);

			DrawLine(list);

			Main.DrawWhip_WhipBland(Projectile, list);
			return false;
		}
	}
}
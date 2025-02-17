using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

/// <summary>
/// Mimics a held staff so it looks like the player is dynamically using the item. Purely visual.
/// </summary>
internal class StaffHeldProjectile : ModProjectile
{
	/// <summary>
	/// Replace texture with empty texture as this uses item textures.
	/// </summary>
	public override string Texture => "Terraria/Images/NPC_0";

	protected Player Owner => Main.player[Projectile.owner];

	public ref float ItemId => ref Projectile.ai[0];

	public override void SetDefaults()
	{
		Projectile.Size = new(1);
		Projectile.friendly  = true;
		Projectile.hostile = false;
		Projectile.timeLeft = 3000;

		Projectile.alpha = 255;
	}

	public override void OnSpawn(IEntitySource source)
	{
		float rotation = Owner.direction == -1 ? MathHelper.Pi : MathHelper.PiOver2;

		Projectile.rotation = rotation;
	}

	public override void AI()
	{
		Owner.heldProj = Projectile.whoAmI;

		// The actual owner's direction is not consistent because the item has useTurn set to true.
		int direction = Math.Sign(Main.MouseWorld.X - Owner.Center.X);
		
		float armRotation = Projectile.rotation + MathHelper.ToRadians(135f);

		if (direction == 1)
		{
			armRotation += MathHelper.Pi;
		}
		
		Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
		Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
		
		if (!Owner.channel)
		{
			Projectile.alpha += 51;

			if (Projectile.alpha >= 255)
			{
				Projectile.Kill();
			}

			return;
		}

		if (Projectile.alpha > 0)
		{
			Projectile.alpha -= 17;
		}

		Projectile.Center = Owner.Center + new Vector2(-10f * -direction, 10f);

		if (!Owner.mount.Active)
		{
			Projectile.Center += Owner.RotatedRelativePoint(Vector2.Zero);
		}
		
		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.rotation = Projectile.rotation.AngleLerp(-MathHelper.PiOver4, 0.5f);
			
			Owner.direction = Math.Sign(Main.MouseWorld.X - Owner.Center.X);

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
			}
		}
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(Projectile.rotation);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		Projectile.rotation = reader.ReadSingle();
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Item[(int)ItemId].Value;

		Vector2 positionOffset = new(Projectile.ModProjectile?.DrawOffsetX ?? 0f, Projectile.gfxOffY);
		Vector2 position = Projectile.Center - Main.screenPosition + positionOffset;

		Vector2 originOffset = new(Projectile.ModProjectile?.DrawOriginOffsetX ?? 0f, Projectile.ModProjectile?.DrawOriginOffsetY ?? 0f);
		Vector2 origin = new Vector2(0f, tex.Height) + originOffset;
		
		Main.spriteBatch.Draw(tex, position, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
		
		return false;
	}
}

using System.IO;
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
	}

	public override void AI()
	{
		Owner.heldProj = Projectile.whoAmI;

		if (!Owner.channel)
		{
			Projectile.Kill();
			return;
		}

		Projectile.Center = Owner.Center + Owner.RotatedRelativePoint(Vector2.Zero);

		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.rotation = Projectile.AngleTo(Main.MouseWorld) + MathHelper.PiOver4;
			Owner.direction = Main.MouseWorld.X <= Owner.Center.X ? -1 : 1;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
			}
		}

		Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.PiOver4 - MathHelper.PiOver2);
		Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.PiOver4 - MathHelper.PiOver2);
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

		Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(0, tex.Height), 1f, SpriteEffects.None, 0);
		return false;
	}
}

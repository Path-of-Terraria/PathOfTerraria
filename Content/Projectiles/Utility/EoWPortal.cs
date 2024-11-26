using PathOfTerraria.Common.Subworlds.BossDomains;
using SubworldLibrary;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class EoWPortal : ModProjectile
{
	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(20, 48);
		Projectile.Opacity = 0.5f;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
		Projectile.velocity *= 0.96f;

		if (Main.rand.NextBool(14))
		{
			Vector2 vel = new Vector2(-Main.rand.NextFloat(4, 8), 0).RotatedBy(Projectile.rotation);
			Dust.NewDustPerfect(Projectile.Center + new Vector2(8, Main.rand.NextFloat(-16, 16)), DustID.PurpleTorch, vel);
		}

		Lighting.AddLight(Projectile.Center, TorchID.Purple);

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.Hitbox.Intersects(Projectile.Hitbox) && Main.myPlayer == player.whoAmI)
			{
				SubworldSystem.Enter<EaterDomain>();
			}
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(Projectile.rotation);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		Projectile.rotation = reader.ReadSingle();
	}
}

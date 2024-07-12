using PathOfTerraria.Content.Items.Pickups;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal class BatSummon : GrimoireSummon
{
	public override void SetDefaults()
	{
		Projectile.width = 26;
		Projectile.height = 34;
		Projectile.tileCollide = false;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
	}

	public override void AI()
	{
		if (!Channeling)
		{
			Projectile.Kill();
			return;
		}

		Projectile.timeLeft++;

		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.2f;
			
			if (Projectile.velocity.LengthSquared() > 8 * 8)
			{
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 8;
			}
		}
	}

	public override Dictionary<int, int> GetRequiredParts()
	{
		return new Dictionary<int, int>()
		{
			{ ModContent.ItemType<OwlFeather>(), 1}, { ModContent.ItemType<BatWings>(), 2}
		};
	}
}

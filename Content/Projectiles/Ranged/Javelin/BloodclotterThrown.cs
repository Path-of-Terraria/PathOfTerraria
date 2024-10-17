using PathOfTerraria.Content.Skills.Ranged;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Ranged.Javelin;

internal class BloodclotterThrown() : JavelinThrown("BloodclotterThrown", new(116), DustID.Blood)
{
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.CanBeChasedBy())
		{
			target.GetGlobalNPC<SiphonNPC>().ApplyStack(Projectile.owner);
		}
	}

	public override void PostAI()
	{
		if (Main.rand.NextBool(6))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Scale: Main.rand.NextFloat(1.2f, 1.8f));
		}
	}
}
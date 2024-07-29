using PathOfTerraria.Content.Skills.Melee;
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
}
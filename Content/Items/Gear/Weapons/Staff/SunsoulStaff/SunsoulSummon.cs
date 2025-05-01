using PathOfTerraria.Common.Systems;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff.SunsoulStaff;

public class SunsoulSummon : ModProjectile
{
	private Player Owner => Main.player[Projectile.owner];

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(60);
		Projectile.tileCollide = false;
		Projectile.timeLeft = 2;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 30;
	}

	public override void AI()
	{
		if (!Owner.GetModPlayer<AltUsePlayer>().AltFunctionActive)
		{
			Projectile.Kill();
			return;
		}

		Projectile.timeLeft++;
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Owner.MinionAttackTargetNPC = target.whoAmI;
		Owner.UpdateMinionTarget();
	}
}

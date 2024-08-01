namespace PathOfTerraria.Core.Systems.DisableBuilding;

internal class StopCuttingProjectile : GlobalProjectile
{
	public override bool? CanCutTiles(Projectile projectile)
	{
		if (projectile.owner != 255)
		{
			return Main.player[projectile.owner].GetModPlayer<StopBuildingPlayer>().LastStopBuilding ? false : null;
		}

		return base.CanCutTiles(projectile);
	}

	public override bool PreKill(Projectile projectile, int timeLeft)
	{
		if (projectile.owner != 255 && Main.player[projectile.owner].GetModPlayer<StopBuildingPlayer>().LastStopBuilding)
		{
			// This stops the Sandgun from dropping sand everywhere
			projectile.noDropItem = true;
		}

		return true;
	}
}

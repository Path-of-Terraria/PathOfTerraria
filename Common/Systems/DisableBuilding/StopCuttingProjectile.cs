using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

internal class StopCuttingProjectile : GlobalProjectile
{
	private static bool Cutting = false;

	public override void Load()
	{
		On_Projectile.CutTiles += AddCutCheck;
		IL_Projectile.CutTilesAt += AddNewCheck;
		On_DelegateMethods.CutTiles += CutCheck; // Unsure if this is needed; most projectiles don't use this.
	}

	private void AddNewCheck(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchCall<WorldGen>(nameof(WorldGen.CanCutTile))))
		{
			return;
		}

		ILLabel label = null;

		if (!c.TryGotoPrev(MoveType.After, x => x.MatchBrtrue(out label)))
		{
			return;
		}

		c.Emit(OpCodes.Ldarg_0);
		c.Emit(OpCodes.Ldloc_S, (byte)5);
		c.Emit(OpCodes.Ldloc_S, (byte)6);
		c.EmitDelegate(CanCutTile);
		c.Emit(OpCodes.Brfalse, label);
	}

	public static bool CanCutTile(Projectile projectile, int i, int j)
	{
		bool cantCutWhitelist = Main.player[projectile.owner].GetModPlayer<StopBuildingPlayer>().LastStopBuilding && !BuildingWhitelist.InCuttingWhitelist(Main.tile[i, j].TileType);
		return !cantCutWhitelist && (ModContent.GetModTile(Main.tile[i, j].TileType) is not ICanCutTile cutTile || cutTile.CanCut(i, j));
	}

	private bool CutCheck(On_DelegateMethods.orig_CutTiles orig, int x, int y)
	{
		bool vanilla = orig(x, y);

		if (Cutting && SubworldSystem.Current is BossDomainSubworld domain)
		{
			return vanilla && BuildingWhitelist.InCuttingWhitelist(Main.tile[x, y].TileType);
		}

		return vanilla;
	}

	private void AddCutCheck(On_Projectile.orig_CutTiles orig, Projectile self)
	{
		Cutting = true;
		orig(self);
		Cutting = false;
	}

	public override bool PreKill(Projectile projectile, int timeLeft)
	{
		if (projectile.owner != 255 && Main.player[projectile.owner].GetModPlayer<StopBuildingPlayer>().LastStopBuilding)
		{
			if (projectile.type == ProjectileID.SandBallGun)
			{
				// This stops the Sandgun from dropping sand everywhere
				projectile.noDropItem = true;
			}
		}

		return true;
	}
}

using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace PathOfTerraria.Common.Projectiles;

internal interface IOnContinuouslyUpdateDamage
{
	public void OnContinuouslyUpdateDamage();
}

public class ContinuouslyUpdateDamageFunctionality : ModSystem
{
	public override void Load()
	{
		IL_Projectile.Update += AddContinuousDamageHook;
	}

	private void AddContinuousDamageHook(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(Projectile).GetProperty(nameof(Projectile.ArmorPenetration)).GetSetMethod())))
		{
			return;
		}

		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate(ContinuousDamageCheck);
	}

	private static void ContinuousDamageCheck(Projectile projectile)
	{
		if (projectile.ModProjectile is IOnContinuouslyUpdateDamage modCont)
		{
			modCont.OnContinuouslyUpdateDamage();
		}

		foreach (GlobalProjectile gProj in projectile.Globals)
		{
			if (gProj is IOnContinuouslyUpdateDamage gCont)
			{
				gCont.OnContinuouslyUpdateDamage();
			}
		}
	}
}

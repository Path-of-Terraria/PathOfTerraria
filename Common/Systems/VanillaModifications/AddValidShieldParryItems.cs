using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.VanillaModifications;

internal class AddValidShieldParryItems : ModSystem
{
	private static readonly HashSet<int> ValidShieldParryItems = [];

	private static Projectile storedProj = null;

	public override void Load()
	{
		IL_Player.ItemCheck_ManageRightClickFeatures_ShieldRaise += AddModdedShieldRaiseItem;
		IL_Player.Update_NPCCollision += HijackParryFunctionality;
		On_Player.CanParryAgainst += On_Player_CanParryAgainst;
		On_Projectile.Damage += StoreProjectileForParry;
	}

	private void StoreProjectileForParry(On_Projectile.orig_Damage orig, Projectile self)
	{
		storedProj = self;
		orig(self);
		storedProj = null;
	}

	private bool On_Player_CanParryAgainst(On_Player.orig_CanParryAgainst orig, Player self, Rectangle blockingPlayerRect, Rectangle enemyRect, Vector2 enemyVelocity)
	{
		bool original = orig(self, blockingPlayerRect, enemyRect, enemyVelocity);

		// Exit early if we're not parrying a projectile - HijackParryFunctionality handles contact damage parrying
		if (storedProj is null)
		{
			return original;
		}

		if (self.HeldItem.ModItem is IParryItem parry)
		{
			if (parry.GetParryCondition(self))
			{
				return parry.ParryProjectile(self, storedProj);
			}
		}

		return original;
	}

	private static void HijackParryFunctionality(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(MoveType.After, x => x.MatchCall<Player>(nameof(Player.CanParryAgainst))))
		{
			return;
		}

		// For some reason vanilla's CanParryAgainst isn't stored to a local
		// So we just take it off the stack then put it back on
		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate(ModifyParry);
	}

	private static bool ModifyParry(bool originalValue, Player self)
	{
		if (self.HeldItem.ModItem is IParryItem parry)
		{
			return parry.GetParryCondition(self);
		}

		return originalValue;
	}

	private static void AddModdedShieldRaiseItem(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchLdfld<Player>(nameof(Player.shield_parry_cooldown))))
		{
			return;
		}

		c.Emit(OpCodes.Ldarg_S, (byte)0);
		c.Emit(OpCodes.Ldarg_S, (byte)1);
		c.Emit(OpCodes.Ldloc_S, (byte)0);
		c.Emit(OpCodes.Ldloca_S, (byte)1);
		c.EmitDelegate(ModifyCanParry);
	}

	private static void ModifyCanParry(Player self, bool theGeneralCheck, bool mouseRight, ref bool shouldGuard)
	{
		if (!ValidShieldParryItems.Contains(self.HeldItem.type))
		{
			return;
		}

		bool canRaiseShield = true;
		IParryItem parryItem = null;

		if (self.HeldItem.ModItem is IParryItem)
		{
			parryItem = self.HeldItem.ModItem as IParryItem;
			canRaiseShield = parryItem.CanRaiseShield(self);
		}

		if (canRaiseShield && theGeneralCheck && self.hasRaisableShield && !self.mount.Active && (self.itemAnimation == 0 || mouseRight))
		{
			shouldGuard = true;
			parryItem?.OnRaiseShield(self);
		}
	}

	internal static void AddParryItem(int type)
	{
		ValidShieldParryItems.Add(type);
	}
}

public interface IParryItem
{
	/// <summary>
	/// Whether the player can raise the shield or not.
	/// </summary>
	/// <param name="player">The player that is trying to raise their shield.</param>
	/// <returns>Whether the player can raise the shield.</returns>
	bool CanRaiseShield(Player player);

	/// <summary>
	/// Called when raising the shield successfully. Note: this is called every frame the shield is up, not just the first time.
	/// </summary>
	/// <param name="player">The player raising the shield.</param>
	void OnRaiseShield(Player player);

	/// <summary>
	/// Whether the player can properly parry or not, instead of just using vanilla's +20 defense boost.
	/// </summary>
	/// <param name="player">The player raising the shield.</param>
	/// <returns>If the player can parry or not.</returns>
	bool GetParryCondition(Player player);

	/// <summary>
	/// Called when trying to parry a projectile. Must succeed a <see cref="GetParryCondition(Player)"/> to run.
	/// </summary>
	/// <param name="player">The player who's attempting to parry.</param>
	/// <param name="projectile">The projectile being parried.</param>
	/// <returns>If the projectile should not deal damage as a result of the parry.</returns>
	bool ParryProjectile(Player player, Projectile projectile);
}
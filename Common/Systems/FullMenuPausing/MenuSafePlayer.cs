namespace PathOfTerraria.Common.Systems.FullMenuPausing;

public class MenuSafePlayer : ModPlayer
{
	/// <summary>
	/// Prevents the player from being hit by NPCs.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="cooldownSlot"></param>
	/// <returns></returns>
	public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
		return !MenuSafeSystem.MenuOpen;
	}

	/// <summary>
	/// Prevents the player from being hit by projectiles.
	/// </summary>
	/// <param name="proj"></param>
	/// <returns></returns>
	public override bool CanBeHitByProjectile(Projectile proj) {
		return !MenuSafeSystem.MenuOpen;
	}

	/// <summary>
	/// Turns off damage and knockback when in a menu.
	/// </summary>
	/// <param name="modifiers"></param>
	public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
		if (MenuSafeSystem.MenuOpen) {
			modifiers.FinalDamage *= 0f;
			modifiers.Knockback *= 0f;
			modifiers.DisableSound();
			modifiers.SetMaxDamage(0);
		}
	}
	
	/// <summary>
	/// Updates life regen when in a menu.
	/// </summary>
	public override void UpdateBadLifeRegen() {
		if (MenuSafeSystem.MenuOpen) {
			Player.lifeRegen = 0;
			Player.lifeRegenTime = 0;
		}
	}

	/// <summary>
	/// Makes player immune while menu is open.
	/// </summary>
	public override void PostUpdate() {
		if (MenuSafeSystem.MenuOpen) {
			Player.immune = true;
			Player.immuneTime = 2;
			Player.noKnockback = true;
			Player.velocity *= 0.01f;
		}
	}
}
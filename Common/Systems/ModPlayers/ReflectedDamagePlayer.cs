namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// Handles dealing thorns-like reflected damage to attacking enemies. 
/// This doesn't use <see cref="Player.thorns"/> as that deals %-based damage and is rather hardcoded.<br/>
/// Note that this doesn't modify Shield of Cthulhu or similar bashing weapons.
/// </summary>
internal class ReflectedDamagePlayer : ModPlayer
{
	public int ReflectedDamage = 0;

	public override void Load()
	{
		On_Player.Update_NPCCollision += StopVanillaThorns;
	}

	/// <summary>
	/// Stops vanilla thorns effects from occuring. This unsets all relevant values, then resets them later.
	/// </summary>
	private static void StopVanillaThorns(On_Player.orig_Update_NPCCollision orig, Player self)
	{
		(float oldThorns, bool oldTurtle, bool oldCactus) = (self.thorns, self.turtleThorns, self.cactusThorns);
		(self.thorns, self.turtleThorns, self.cactusThorns) = (0, false, false);
		orig(self);
		(self.thorns, self.turtleThorns, self.cactusThorns) = (oldThorns, oldTurtle, oldCactus);
	}

	public override void ResetEffects()
	{
		ReflectedDamage = 0;
	}

	public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
	{
		if (ReflectedDamage > 0)
		{
			int dir = Player.Center.X < npc.Center.X ? 1 : -1;
			npc.SimpleStrikeNPC(ReflectedDamage, dir, false, 1 * (ReflectedDamage / 20f));
		}
	}

	public static float GetVanillaThornsModifier(Player player)
	{
		// Add vanilla's Cactus damage as a base number.
		if (player.cactusThorns)
		{
			int damage = 15;

			if (Main.masterMode)
			{
				damage = 45;
			}
			else if (Main.expertMode)
			{
				damage = 30;
			}

			player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ReflectedDamageModifier.Base += damage;
		}

		// Return multiplier on thorns, which is 2 when wearing full melee Turtle Armor, or player.thorns otherwise.
		// These values are approximated as they don't perfectly line up with how vanilla's system works.
		if (player.turtleThorns)
		{
			player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ReflectedDamageModifier.Base += player.thorns * 40;
			return 3;
		}

		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ReflectedDamageModifier.Base += player.thorns * 20;
		return player.thorns + 1;
	}
}

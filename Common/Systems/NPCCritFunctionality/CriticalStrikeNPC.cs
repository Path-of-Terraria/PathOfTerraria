namespace PathOfTerraria.Common.Systems.NPCCritFunctionality;

internal class CriticalStrikeNPC : GlobalNPC
{
	public static bool CurrentlyCritting = false;

	public override bool InstancePerEntity => true;

	/// <summary>
	/// Critical strike chance for this NPC. Defaults to 0.05 (5%).
	/// </summary>
	public AddableFloat CriticalStrikeChance = new AddableFloat() + 0.05f;

	public override void Load()
	{
		On_Player.Hurt_HurtInfo_bool += UnsetCrit;
	}

	private void UnsetCrit(On_Player.orig_Hurt_HurtInfo_bool orig, Player self, Player.HurtInfo info, bool quiet)
	{
		orig(self, info, quiet);

		CurrentlyCritting = false;
	}

	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
	{
		return !entity.friendly;
	}

	public override void ModifyHitPlayer(NPC npc, Player player, ref Player.HurtModifiers modifiers)
	{
		if (Main.rand.NextFloat() < CriticalStrikeChance.Value * 3000)
		{
			modifiers.FinalDamage += 1.5f;
			CurrentlyCritting = true;
		}
	}
}

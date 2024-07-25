using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Common.Buffs;

public sealed class NPCRage : GlobalNPC
{
	public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
	{
		if (!target.HasBuff<Rage>())
		{
			return;
		}
		
		modifiers.FinalDamage += 5;
	}
}
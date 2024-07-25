using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Common.Buffs;

public class NPCBattleaxe : GlobalNPC
{
	public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
	{
		if (!target.HasBuff<Battleaxe>())
		{
			return;
		}
		
		modifiers.FinalDamage += 3;
		modifiers.Knockback += 2;
	}
}
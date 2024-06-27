using PathOfTerraria.Core.Events;

namespace PathOfTerraria.Content.Buffs;

class RageBuff() : SmartBuff(false)
{
	public override void Load()
	{
		PathOfTerrariaNpcEvents.ModifyHitPlayerEvent += TakeMoreDamage;
	}

	private void TakeMoreDamage(NPC npc, Player target, ref Player.HurtModifiers modifiers)
	{
		modifiers.FinalDamage += 5;
	}

	public override void Update(Player player, ref int buffIndex)
	{ 
		player.GetDamage(DamageClass.Generic) += 1.5f; //150% more damage
	}
}
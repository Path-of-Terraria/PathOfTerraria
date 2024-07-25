using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Common.Debuffs;

public sealed class NPCArmorShred : GlobalNPC
{
	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		if (!npc.HasBuff(ModContent.BuffType<ArmorShred>())) 
		{
			return;
		}
		
		modifiers.Defense *= ArmorShred.DefenseMultiplier;
	}
	
	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		if (!npc.HasBuff(ModContent.BuffType<ArmorShred>()))
		{
			return;
		}
		
		modifiers.Defense *= ArmorShred.DefenseMultiplier;
	}
}
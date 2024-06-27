using PathOfTerraria.Core.Events;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Buffs;

public class ArmorShredDebuff() : SmartBuff(false)
{
	private const int DefenseReductionPercent = 25;
	private static readonly float DefenseMultiplier = 1 - DefenseReductionPercent / 100f;
	public new const int Type = 388;
	
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Buffs/ArmorShredDebuff";
	
	public float GetDefenceModifer()
	{
		return DefenseMultiplier;
	}
}

public class ArmorShredDebuffNpc : GlobalNPC
{
	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		if (npc.HasBuff(ModContent.BuffType<ArmorShredDebuff>())) {
			modifiers.Defense *= new ArmorShredDebuff().GetDefenceModifer();
		}
	}
	
	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		if (npc.HasBuff(ModContent.BuffType<ArmorShredDebuff>())) {
			modifiers.Defense *= new ArmorShredDebuff().GetDefenceModifer();
		}
	}
}
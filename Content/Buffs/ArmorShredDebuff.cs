﻿namespace PathOfTerraria.Content.Buffs;

public class ArmorShredDebuff : ModBuff
{
	private const int DefenseReductionPercent = 25;
	public static readonly float DefenseMultiplier = 1 - DefenseReductionPercent / 100f;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.debuff[Type] = true;
	}
}

public class ArmorShredDebuffNpc : GlobalNPC
{
	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		if (npc.HasBuff(ModContent.BuffType<ArmorShredDebuff>())) {
			modifiers.Defense *= ArmorShredDebuff.DefenseMultiplier;
		}
	}
	
	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		if (npc.HasBuff(ModContent.BuffType<ArmorShredDebuff>())) {
			modifiers.Defense *= ArmorShredDebuff.DefenseMultiplier;
		}
	}
}
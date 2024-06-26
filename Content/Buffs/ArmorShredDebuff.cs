﻿using PathOfTerraria.Core.Events;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Buffs;

public class ArmorShredDebuff() : SmartBuff(false)
{
	private const int DefenseReductionPercent = 25;
	public static readonly float DefenseMultiplier = 1 - DefenseReductionPercent / 100f;
	
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Buffs/ArmorShredDebuff";
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
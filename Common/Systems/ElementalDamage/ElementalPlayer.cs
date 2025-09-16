using System.Linq;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

public class ElementalPlayer : ModPlayer
{
	public ElementalContainer Container = new();

	// TODO: could be a ModConfig toggle
	public static bool DebugMessages => false;

	public override void ResetEffects()
	{
		Container.Reset(true);
	}

	public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
	{
		if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj))
		{
			ElementModifyDamage(elemProj.Container, Container, ref modifiers.IncomingDamageMultiplier, ref modifiers.SourceDamage);
		}
	}

	public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementModifyDamage(elemNPC.Container, Container, ref modifiers.IncomingDamageMultiplier, ref modifiers.SourceDamage);
		}
	}

	private static void ElementModifyDamage(ElementalContainer container, ElementalContainer other, ref MultipliableFloat preDefenseMultiplier, ref StatModifier sourceDamage)
	{
		// Apply multiplier to original damage BEFORE DEFENSE, by each element conversion percent (accounting for resistance) 
		float totalMultiplier = container[ElementType.Fire].GetTotalConversion(other) + container[ElementType.Cold].GetTotalConversion(other) 
			+ container[ElementType.Lightning].GetTotalConversion(other) + container[ElementType.Chaos].GetTotalConversion(other) + (1f - container.TotalConversion);
		preDefenseMultiplier *= totalMultiplier;

		// Apply flat extra damage (accounting for resistance)
		foreach (ElementInstance element in container)
		{
			sourceDamage.Flat += element.GetFlatDamage(other);
		}

		if (DebugMessages && (container.Any(x => x.DamageModifier.HasValues)))
		{
			Main.NewText("[DEBUG] Elemental Damage Modifiers:");

			foreach (ElementInstance element in container)
			{
				ElementalDamage mod = element.DamageModifier;
				Main.NewText($"{element.Type}: Conversion: {mod.DamageConversion * 100}%, Flat: {mod.DamageBonus:0.##}, Resistance: {element.Resistance * 100}%");
			}
			
			Main.NewText($"  Total Multiplier Applied: {totalMultiplier:0.####}");
		}
	}

	public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
	{
		if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj))
		{
			ElementOnHit(Player, elemProj.Container, Container, hurtInfo.SourceDamage, hurtInfo.Damage, null);
		}
	}

	public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementOnHit(Player, elemNPC.Container, Container, hurtInfo.SourceDamage, hurtInfo.Damage, null);
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementOnHit(target, Container, Container, hit.SourceDamage, hit.Damage, hit);
		}
	}

	private static void ElementOnHit(Entity target, ElementalContainer container, ElementalContainer other, float sourceDamage, int finalDamage, NPC.HitInfo? optionalHitInfo)
	{
		float baseFraction = 1f - container.TotalConversion;
		float total = container.Sum(x => x.GetTotalConversion(other)) + baseFraction;

		float originalDamage = sourceDamage - container.Sum(x => x.GetFlatDamage(other));
		float baseOriginal = originalDamage * (baseFraction / total);
		float totalOriginal = container.Sum(x => originalDamage * (x.GetTotalConversion(other) / total) + x.GetFlatDamage(other)) + baseOriginal;

		// Elemental damage done, rounded down
		foreach (ElementInstance element in container)
		{
			float original = originalDamage * (element.GetTotalConversion(other) / total) + element.GetFlatDamage(other);
			int damage = (int)(finalDamage * (original / totalOriginal));

			if (DebugMessages && damage > 0)
			{
				Main.NewText($"  {element.Type}:	{damage}");
			}

			// Apply the buff if the hit info exists (we hit an NPC)
			if (optionalHitInfo is not null)
			{
				TryAddElementBuff(target, element.DamageModifier, damage, optionalHitInfo.Value);
			}
		}
	}

	private static bool TryAddElementBuff(Entity target, ElementalDamage damage, int elementalDamageDone, NPC.HitInfo hitInfo)
	{
		if (!damage.HasValues)
		{
			return false;
		}

		int lifeMax = target switch
		{
			NPC npc => npc.lifeMax,
			Player player => player.statLifeMax2,
			_ => throw new ArgumentException("target should be an NPC or Player!")
		};

		float chance = ElementalDamage.GetDebuffChance((float)elementalDamageDone / lifeMax);

		if (elementalDamageDone > 0 && damage.CanDebuff(target, hitInfo, chance < Main.rand.NextFloat()))
		{
			damage.ApplyBuff(target, elementalDamageDone);

			if (DebugMessages)
			{
				Main.NewText($"[DEBUG] Debuff for {damage.ElementType} applied!");
			}

			return true;
		}

		return false;
	}
}

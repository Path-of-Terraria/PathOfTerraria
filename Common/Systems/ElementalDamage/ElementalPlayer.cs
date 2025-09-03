namespace PathOfTerraria.Common.Systems.ElementalDamage;

public class ElementalPlayer : ModPlayer
{
	public ElementalContainer Container = new();

	// TODO: could be a ModConfig toggle
	public static bool DebugMessages => true;

	public override void ResetEffects()
	{
		Container.Reset(true);
	}

	public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
	{
		if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj))
		{
			ElementModifyDamage(elemProj.Container, ref modifiers.IncomingDamageMultiplier, ref modifiers.SourceDamage);
		}
	}

	public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementModifyDamage(elemNPC.Container, ref modifiers.IncomingDamageMultiplier, ref modifiers.SourceDamage);
		}
	}

	private static void ElementModifyDamage(ElementalContainer container, ref MultipliableFloat preDefenseMultiplier, ref StatModifier sourceDamage)
	{
		ElementalDamage fire = container.FireDamageModifier;
		ElementalDamage cold = container.ColdDamageModifier;
		ElementalDamage light = container.LightningDamageModifier;

		// Apply multiplier to original damage BEFORE DEFENSE, by each element conversion percent (accounting for resistance) 
		float totalMultiplier = fire.DamageConversion * (1f - container.FireResistance) + cold.DamageConversion * (1f - container.ColdResistance) + light.DamageConversion 
			* (1f - container.LightningResistance) + (1f - container.TotalConversion);
		preDefenseMultiplier *= totalMultiplier;

		// Apply flat extra damage (accounting for resistance)
		sourceDamage.Flat += fire.DamageBonus * (1f - container.FireResistance);
		sourceDamage.Flat += cold.DamageBonus * (1f - container.ColdResistance);
		sourceDamage.Flat += light.DamageBonus * (1f - container.LightningResistance);

		if (DebugMessages && (fire.Valid || cold.Valid || light.Valid))
		{
			Main.NewText("[DEBUG] Elemental Damage Modifiers:");
			Main.NewText($"  Fire:      Conversion: {fire.DamageConversion * 100}%, Flat: {fire.DamageBonus:0.##}, Resistance: {container.FireResistance * 100}%");
			Main.NewText($"  Cold:      Conversion: {cold.DamageConversion * 100}%, Flat: {cold.DamageBonus:0.##}, Resistance: {container.ColdResistance * 100}%");
			Main.NewText($"  Lightning: Conversion: {light.DamageConversion * 100}%, Flat: {light.DamageBonus:0.##}, Resistance: {container.LightningResistance * 100}%");
			Main.NewText($"  Total Multiplier Applied: {totalMultiplier:0.####}");
		}
	}

	public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
	{
		if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj))
		{
			ElementOnHit(Player, elemProj.Container, hurtInfo.SourceDamage, hurtInfo.Damage);
		}
	}

	public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementOnHit(Player, elemNPC.Container, hurtInfo.SourceDamage, hurtInfo.Damage);
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC)) 
		{
			ElementOnHit(target, Container, hit.SourceDamage, hit.Damage);
		}
	}

	private void ElementOnHit(Entity target, ElementalContainer container, float sourceDamage, int finalDamage)
	{
		ElementalDamage fire = container.FireDamageModifier;
		ElementalDamage cold = container.ColdDamageModifier;
		ElementalDamage light = container.LightningDamageModifier;

		// Flat bonus, with resistance
		float fireFlat = fire.DamageBonus * (1f - container.FireResistance);
		float coldFlat = cold.DamageBonus * (1f - container.ColdResistance);
		float lightningFlat = light.DamageBonus * (1f - container.LightningResistance);

		// Conversion % accounting for resistance
		float fireFraction = fire.DamageConversion * (1f - container.FireResistance);
		float coldFraction = cold.DamageConversion * (1f - container.ColdResistance);
		float lightningFraction = light.DamageConversion * (1f - container.LightningResistance);
		float baseFraction = 1f - container.TotalConversion;
		float total = fireFraction + coldFraction + lightningFraction + baseFraction;

		// Original damage (pre-defense, pre-resistance) without flat bonus
		float originalDamage = sourceDamage - (fireFlat + coldFlat + lightningFlat);

		float fireOriginal = originalDamage * (fireFraction / total) + fireFlat;
		float coldOriginal = originalDamage * (coldFraction / total) + coldFlat;
		float lightningOriginal = originalDamage * (lightningFraction / total) + lightningFlat;
		float baseOriginal = originalDamage * (baseFraction / total);

		float totalOriginal = fireOriginal + coldOriginal + lightningOriginal + baseOriginal;

		// Elemental damage done, rounded down
		int fireDamage = (int)(finalDamage * (fireOriginal / totalOriginal));
		int coldDamage = (int)(finalDamage * (coldOriginal / totalOriginal));
		int lightningDamage = (int)(finalDamage * (lightningOriginal / totalOriginal));

		if (DebugMessages && (fireDamage > 0 || coldDamage > 0 || lightningDamage > 0))
		{
			Main.NewText("[DEBUG] Elemental Damage Done:");
			Main.NewText($"  Fire:         {fireDamage}");
			Main.NewText($"  Cold:         {coldDamage}");
			Main.NewText($"  Lightning:    {lightningDamage}");
			Main.NewText($"  Total:        {finalDamage}");
			Main.NewText($"  Original dmg: {(int)(finalDamage * ((sourceDamage - (fireFlat + coldFlat + lightningFlat)) / sourceDamage))}");
		}

		TryAddElementBuff(target, fire, fireDamage);
		TryAddElementBuff(target, cold, coldDamage);
		TryAddElementBuff(target, light, lightningDamage);
	}

	private bool TryAddElementBuff(Entity target, ElementalDamage damage, int elementalDamageDone)
	{
		if (!damage.Valid)
		{
			return false;
		}

		int buffType = damage.GetBuffType();
		float chance = damage.GetDebuffChance((float)elementalDamageDone / Player.statLifeMax2) * 1000;

		if (buffType > 0 && buffType <= BuffLoader.BuffCount && Main.rand.NextFloat() < chance)
		{
			int timeToAdd = damage.GetBuffDuration();
			damage.ApplyBuff(target, buffType, timeToAdd, elementalDamageDone);

			if (DebugMessages)
			{
				Main.NewText($"[DEBUG] Debuff {buffType} applied for {timeToAdd} ticks!");
			}

			return true;
		}

		return false;
	}
}

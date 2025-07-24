namespace PathOfTerraria.Common.Systems.ElementalDamage;

public class ElementalPlayer : ModPlayer
{

	private float _fireResistance;
	public float FireResistance
	{
		get => _fireResistance;
		set => _fireResistance = MathHelper.Clamp(value, 0f, 0.75f);
	}

	private float _coldResistance;
	public float ColdResistance
	{
		get => _coldResistance;
		set => _coldResistance = MathHelper.Clamp(value, 0f, 0.75f);
	}

	private float _lightningResistance;
	public float LightningResistance
	{
		get => _lightningResistance;
		set => _lightningResistance = MathHelper.Clamp(value, 0f, 0.75f);
	}

	// TODO: could be a ModConfig toggle
	public static bool DebugMessages => false;

	public override void ResetEffects()
	{
		FireResistance = 0f;
		ColdResistance = 0.5f;
		LightningResistance = 0f;
	}

	public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
	{
		if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj))
		{
			ElementModifyDamage(elemProj.FireDamage, elemProj.ColdDamage, elemProj.LightningDamage, ref modifiers.IncomingDamageMultiplier, ref modifiers.SourceDamage);
		}
	}

	public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementModifyDamage(elemNPC.FireDamage, elemNPC.ColdDamage, elemNPC.LightningDamage, ref modifiers.IncomingDamageMultiplier, ref modifiers.SourceDamage);
		}
	}

	private void ElementModifyDamage(ElementalDamage fire, ElementalDamage cold, ElementalDamage lightning, ref MultipliableFloat preDefenseMultiplier, ref StatModifier sourceDamage)
	{
		float totalConversion = MathHelper.Clamp(fire.DamageConversion + cold.DamageConversion + lightning.DamageConversion, 0f, 1f);

		// Apply multiplier to original damage BEFORE DEFENSE, by each element conversion percent (accounting for resistance) 
		float totalMultiplier = fire.DamageConversion * (1f - FireResistance) + cold.DamageConversion * (1f - ColdResistance) + lightning.DamageConversion * (1f - LightningResistance) + (1f - totalConversion);
		preDefenseMultiplier *= totalMultiplier;

		// Apply flat extra damage (accounting for resistance)
		sourceDamage.Flat += fire.DamageBonus * (1f - FireResistance);
		sourceDamage.Flat += cold.DamageBonus * (1f - ColdResistance);
		sourceDamage.Flat += lightning.DamageBonus * (1f - LightningResistance);

		if (DebugMessages && (fire.Valid || cold.Valid || lightning.Valid))
		{
			Main.NewText("[DEBUG] Elemental Damage Modifiers:");
			Main.NewText($"  Fire:      Conversion: {fire.DamageConversion * 100}%, Flat: {fire.DamageBonus:0.##}, Resistance: {FireResistance * 100}%");
			Main.NewText($"  Cold:      Conversion: {cold.DamageConversion * 100}%, Flat: {cold.DamageBonus:0.##}, Resistance: {ColdResistance * 100}%");
			Main.NewText($"  Lightning: Conversion: {lightning.DamageConversion * 100}%, Flat: {lightning.DamageBonus:0.##}, Resistance: {LightningResistance * 100}%");
			Main.NewText($"  Total Multiplier Applied: {totalMultiplier:0.####}");
		}
	}

	public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
	{
		if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj))
		{
			ElementOnHit(Player, elemProj.FireDamage, elemProj.ColdDamage, elemProj.LightningDamage, hurtInfo.SourceDamage, hurtInfo.Damage);
		}
	}

	public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementOnHit(Player, elemNPC.FireDamage, elemNPC.ColdDamage, elemNPC.LightningDamage, hurtInfo.SourceDamage, hurtInfo.Damage);
		}
	}

	private void ElementOnHit(Entity target, ElementalDamage fire, ElementalDamage cold, ElementalDamage lightning, float sourceDamage, int finalDamage)
	{
		// Conversion %
		float totalConversion = MathHelper.Clamp(fire.DamageConversion + cold.DamageConversion + lightning.DamageConversion, 0f, 1f);

		// Flat bonus, with resistance
		float fireFlat = fire.DamageBonus * (1f - FireResistance);
		float coldFlat = cold.DamageBonus * (1f - ColdResistance);
		float lightningFlat = lightning.DamageBonus * (1f - LightningResistance);

		// Conversion % accounting for resistance
		float fireFraction = fire.DamageConversion * (1f - FireResistance);
		float coldFraction = cold.DamageConversion * (1f - ColdResistance);
		float lightningFraction = lightning.DamageConversion * (1f - LightningResistance);
		float baseFraction = 1f - totalConversion;
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
		TryAddElementBuff(target, lightning, lightningDamage);
	}

	private bool TryAddElementBuff(Entity target, ElementalDamage damage, int elementalDamageDone)
	{
		if (!damage.Valid)
		{
			return false;
		}

		int buffType = damage.GetBuffType();
		float chance = damage.GetDebuffChance((float)elementalDamageDone / Player.statLifeMax2);

		if (buffType > 0 && buffType <= BuffLoader.BuffCount && Main.rand.NextFloat() < chance)
		{
			int timeToAdd = damage.GetBuffDuration();

			if (target is NPC npc)
			{
				npc.AddBuff(buffType, timeToAdd);
			}
			else if (target is Player player)
			{
				player.AddBuff(buffType, timeToAdd);
			}

			if (DebugMessages)
			{
				Main.NewText($"[DEBUG] Debuff {buffType} applied for {timeToAdd} ticks!");
			}

			return true;
		}

		return false;
	}
}

using PathOfTerraria.Common.Systems.DamageTypes;
using PathOfTerraria.Common.Systems.MobSystem;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Systems.ModPlayers;
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
	public bool DebugMessages => true;

	public override void ResetEffects()
	{
		FireResistance = 0f;
		ColdResistance = 0f;
		LightningResistance = 0f;
	}

	public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
	{
		if (npc.TryGetGlobalNPC(out ArpgNPC arpgNPC))
		{
			float fireConversion = arpgNPC.FireDamage?.DamageConversion ?? 0f;
			float coldConversion = arpgNPC.ColdDamage?.DamageConversion ?? 0f;
			float lightningConversion = arpgNPC.LightningDamage?.DamageConversion ?? 0f;
			float totalConversion = MathHelper.Clamp(fireConversion + coldConversion + lightningConversion, 0f, 1f);

			// Apply multiplier to original damage BEFORE DEFENSE, by each element conversion percent (accounting for resistance) 
			float totalMultiplier = fireConversion * (1f - FireResistance) + coldConversion * (1f - ColdResistance) + lightningConversion * (1f - LightningResistance) + (1f - totalConversion);
			modifiers.IncomingDamageMultiplier *= totalMultiplier;

			// Apply flat extra damage (accounting for resistance)
			modifiers.SourceDamage.Flat += (arpgNPC.FireDamage?.DamageBonus ?? 0f) * (1f - FireResistance);
			modifiers.SourceDamage.Flat += (arpgNPC.ColdDamage?.DamageBonus ?? 0f) * (1f - ColdResistance);
			modifiers.SourceDamage.Flat += (arpgNPC.LightningDamage?.DamageBonus ?? 0f) * (1f - LightningResistance);

			if (DebugMessages && (arpgNPC.FireDamage != null || arpgNPC.ColdDamage != null || arpgNPC.LightningDamage != null))
			{
				Main.NewText("[DEBUG] Elemental Damage Modifiers:");
				Main.NewText($"  Fire:      Conversion: {fireConversion * 100}%, Flat: {arpgNPC.FireDamage?.DamageBonus ?? 0f:0.##}, Resistance: {FireResistance:0.##}");
				Main.NewText($"  Cold:      Conversion: {coldConversion * 100}%, Flat: {arpgNPC.ColdDamage?.DamageBonus ?? 0f:0.##}, Resistance: {ColdResistance:0.##}");
				Main.NewText($"  Lightning: Conversion: {lightningConversion * 100}%, Flat: {arpgNPC.LightningDamage?.DamageBonus ?? 0f:0.##}, Resistance: {LightningResistance:0.##}");
				Main.NewText($"  Total Multiplier Applied: {totalMultiplier:0.####}");
			}
		}
	}

	public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
	{
		if (npc.TryGetGlobalNPC(out ArpgNPC arpgNPC))
		{
			// Conversion %
			float fireConversion = arpgNPC.FireDamage?.DamageConversion ?? 0f;
			float coldConversion = arpgNPC.ColdDamage?.DamageConversion ?? 0f;
			float lightningConversion = arpgNPC.LightningDamage?.DamageConversion ?? 0f;
			float totalConversion = MathHelper.Clamp(fireConversion + coldConversion + lightningConversion, 0f, 1f);

			// Flat bonus, with resistance
			float fireFlat = (arpgNPC.FireDamage?.DamageBonus ?? 0f) * (1f - FireResistance);
			float coldFlat = (arpgNPC.ColdDamage?.DamageBonus ?? 0f) * (1f - ColdResistance);
			float lightningFlat = (arpgNPC.LightningDamage?.DamageBonus ?? 0f) * (1f - LightningResistance);

			// Conversion % accounting for resistance
			float fireFraction = fireConversion * (1f - FireResistance);
			float coldFraction = coldConversion * (1f - ColdResistance);
			float lightningFraction = lightningConversion * (1f - LightningResistance);
			float baseFraction = 1f - totalConversion;
			float total = fireFraction + coldFraction + lightningFraction + baseFraction;

			// Original damage (pre-defense, pre-resistance) without flat bonus
			float originalDamage = hurtInfo.SourceDamage - (fireFlat + coldFlat + lightningFlat);

			float fireOriginal = originalDamage * (fireFraction / total) + fireFlat;
			float coldOriginal = originalDamage * (coldFraction / total) + coldFlat;
			float lightningOriginal = originalDamage * (lightningFraction / total) + lightningFlat;
			float baseOriginal = originalDamage * (baseFraction / total);

			float totalOriginal = fireOriginal + coldOriginal + lightningOriginal + baseOriginal;

			// Elemental damage done, rounded down
			int fireDamage = (int)(hurtInfo.Damage * (fireOriginal / totalOriginal));
			int coldDamage = (int)(hurtInfo.Damage * (coldOriginal / totalOriginal));
			int lightningDamage = (int)(hurtInfo.Damage * (lightningOriginal / totalOriginal));

			if (DebugMessages && (fireDamage > 0 || coldDamage > 0 || lightningDamage > 0))
			{
				Main.NewText("[DEBUG] Elemental Damage Done:");
				Main.NewText($"  Fire:         {fireDamage}");
				Main.NewText($"  Cold:         {coldDamage}");
				Main.NewText($"  Lightning:    {lightningDamage}");
				Main.NewText($"  Total:        {hurtInfo.Damage}");
				Main.NewText($"  Original dmg: {hurtInfo.Damage * ((hurtInfo.SourceDamage - (fireFlat + coldFlat + lightningFlat)) / hurtInfo.SourceDamage)}");
			}

			TryAddElementBuff(arpgNPC.FireDamage, fireDamage);
			TryAddElementBuff(arpgNPC.ColdDamage, coldDamage);
			TryAddElementBuff(arpgNPC.LightningDamage, lightningDamage);
		}
	}


	private bool TryAddElementBuff(DamageType damageType, int elementalDamageDone)
	{
		if(damageType == null)
		{
			return false;
		}

		int buffType = damageType.GetBuffType();
		float chance = damageType.GetDebuffChance((float)elementalDamageDone / Player.statLifeMax2);
		if (buffType > 0 && buffType <= BuffLoader.BuffCount && Main.rand.NextFloat() < chance)
		{
			int timeToAdd = damageType.GetBuffDuration();
			Player.AddBuff(buffType, timeToAdd);

			if (DebugMessages)
			{
				Main.NewText($"[DEBUG] Debuff {buffType} applied for {timeToAdd} ticks!");
			}

			return true;
		}

		return false;
	}
}

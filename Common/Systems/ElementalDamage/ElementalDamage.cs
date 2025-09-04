using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

/// <summary>
/// Controls a single element
/// </summary>
public readonly struct ElementalDamage
{
	public ElementType ElementType { get; init; }
	public int DamageBonus { get; init; }
	public float DamageConversion { get; init; }

	public bool Valid
	{
		get
		{
			if (ElementType == ElementType.None)
			{
				int i = 0;
			}

			return ElementType != ElementType.None;
		}
	}

	public ElementalDamage(ElementType elementType)
	{
		ElementType = elementType;
	}

	public ElementalDamage(ElementType elementType, int bonus, float conversion)
	{
		ElementType = elementType;
		DamageBonus = bonus;
		DamageConversion = Math.Clamp(conversion, 0f, 1f);
	}

	public ElementalDamage ApplyOverride(int? newBonus, float? newConversion)
	{
		int damageBonus = DamageBonus;
		float damageConversion = DamageConversion;

		if (newBonus.HasValue)
		{
			damageBonus = newBonus.Value;
		}

		if (newConversion.HasValue)
		{
			damageConversion = Math.Clamp(newConversion.Value, 0f, 1f);
		}

		return new ElementalDamage(ElementType, damageBonus, damageConversion);
	}

	public ElementalDamage AddModifiers(int? newBonus, float? newConversion)
	{
		int damageBonus = DamageBonus;
		float damageConversion = DamageConversion;

		return new ElementalDamage(ElementType, damageBonus + (newBonus ?? 0), damageConversion + (newConversion ?? 0));
	}

	// TODO: could depend on level, damage done, etc. for applying stronger debuffs (e.g. OnFire2, Frostburn, etc.)
	/// <summary> The debuff type to apply. </summary>
	public int GetBuffType()
	{
		return ElementType switch
		{
			ElementType.Fire => ModContent.BuffType<IgnitedDebuff>(),
			ElementType.Cold => BuffID.Frostburn,
			ElementType.Lightning => BuffID.Electrified,
			_ => 0
		};
	}

	/// <summary> The duration of the <see cref="GetBuffType()"/> applied (in ticks) </summary>
	public int GetBuffDuration()
	{
		return ElementType switch
		{
			// values TBD
			ElementType.Fire => 4 * 60,
			ElementType.Cold => 5 * 60,
			ElementType.Lightning => 5 * 60,
			_ => 0
		};
	}

	public void ApplyBuff(Entity entity, int buffType, int timeToAdd, int elementalDamageDealt)
	{
		switch (ElementType)
		{
			case ElementType.Fire when entity is NPC burningNPC:
				IgnitedDebuff.ApplyTo(burningNPC, (int)(elementalDamageDealt * 0.9f));
				break;

			default:
				if (entity is NPC npc)
				{
					npc.AddBuff(buffType, timeToAdd);
				}
				else if (entity is Player player)
				{
					player.AddBuff(buffType, timeToAdd);
				}

				break;
		}
	}

	/// <summary> Get the chance of applying the <see cref="GetBuffType()"/> </summary>
	public float GetDebuffChance(float damagePercent)
	{
		// No chance if damage < 2% of target's max health
		if (damagePercent < 0.02f)
		{
			return 0f;
		}
		// Cap chance at 20% once damage percent >= 50%
		else if (damagePercent >= 0.5f)
		{
			return 0.2f;
		}
		// Linearly increase chance from 0% -> 20% as damage goes from 2% -> 50% of max health
		else
		{
			return damagePercent * (0.2f / 0.5f);
		}
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write((byte)ElementType);
		writer.Write(DamageBonus);
		writer.Write(DamageConversion);
	}

	public static ElementalDamage Read(BinaryReader reader)
	{
		var element = (ElementType)reader.ReadByte();
		int bonus = reader.ReadInt32();
		float conversion = reader.ReadSingle();
		return new ElementalDamage(element, bonus, conversion);
	}

	public override string ToString()
	{
		return $"{ElementType}: Flat: {DamageBonus} Conv: {DamageConversion}";
	}
}


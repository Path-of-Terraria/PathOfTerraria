using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

/// <summary>
/// Controls a single damage element, such as Fire.
/// </summary>
public readonly struct ElementalDamage
{
	/// <summary>
	/// The type that this element refers to. Used to speed up some checks; if this isn't set, some functionality won't work properly.
	/// </summary>
	public ElementType ElementType { get; init; }
	
	/// <summary>
	/// Flat damage bonus. For example, 10 here would deal (base) + 10 damage, and only the 10 would necessarily count as elemental.
	/// </summary>
	public int DamageBonus { get; init; }

	/// <summary>
	/// How much of base damage to convert. 0.5f would convert 50% of a hit's damage to this damage.<br/>
	/// The way this works with multiple damage types is simple: 
	/// per <see cref="ElementalContainer"/>, the <see cref="ElementalContainer.TotalConversion"/> is the % of the base hit that is elemental. The rest is non-elemental.<br/>
	/// For example, 50% Fire and 25% Lightning would deal 50% fire and half of that 50% would be both fire and lightning damage for the use of damage, 
	/// <b>NOT</b> including <see cref="DamageBonus"/>.
	/// </summary>
	public float DamageConversion { get; init; }

	/// <summary>
	/// Whether the object has any non-zero damage-related values. If it doesn't, this struct is "default" and shouldn't be synced or used for further functionality.
	/// </summary>
	public bool HasValues => DamageBonus > 0 || DamageConversion > 0;

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

	/// <summary>
	/// Fully overrides this instance to use the new parameters, if any. <see langword="null"/> on either means that value will not be overriden.<br/>
	/// Note that this method returns the new instance, and doesn't modify the current instance. Not using the return value means nothing is done.
	/// </summary>
	/// <param name="newBonus">The new flat bonus to set, if any.</param>
	/// <param name="newConversion">The new conversion to set, if any.</param>
	/// <returns>The modified value.</returns>
	public ElementalDamage ApplyOverride(int? newBonus, float? newConversion)
	{
		if (!newBonus.HasValue && !newConversion.HasValue)
		{
			return this;
		}

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

	/// <summary>
	/// Returns a new <see cref="ElementalDamage"/> instance which adds in <paramref name="newBonus"/> and <paramref name="newConversion"/>, if applicable.<br/>
	/// This should be used when modifying a value, and should be done once - such as on NPC spawn, or per NPC affix.<br/>
	/// Note that this method returns the new instance, and doesn't modify the current instance. Not using the return value means nothing is done.
	/// </summary>
	/// <param name="newBonus">The new flat bonus to add, if any.</param>
	/// <param name="newConversion">The new conversion to add, if any. This value is clamped between and including 0 and 1.</param>
	/// <returns>The modified value.</returns>
	public ElementalDamage AddModifiers(int? newBonus, float? newConversion)
	{
		int damageBonus = DamageBonus;
		float damageConversion = DamageConversion;

		return new ElementalDamage(ElementType, damageBonus + (newBonus ?? 0), damageConversion + (newConversion ?? 0));
	}

	/// <summary> 
	/// The debuff type to apply. Functionality is in <see cref="ApplyBuff(Entity, int)"/>, which may not call this method. 
	/// </summary>
	public static int GetBuffType(ElementType type)
	{
		return type switch
		{
			ElementType.Fire => ModContent.BuffType<IgnitedDebuff>(),
			ElementType.Cold => BuffID.Frostburn,
			ElementType.Lightning => BuffID.Electrified,
			_ => 0
		};
	}

	public void ApplyBuff(Entity entity, int elementalDamageDealt)
	{
		switch (ElementType)
		{
			case ElementType.Fire when entity is NPC burningNPC:
				IgnitedDebuff.ApplyTo(burningNPC, (int)(elementalDamageDealt * 0.9f));
				break;

			default:
				const int MaxTime = 5 * 40;

				if (entity is NPC npc)
				{
					npc.AddBuff(GetBuffType(ElementType), MaxTime);
				}
				else if (entity is Player player)
				{
					player.AddBuff(GetBuffType(ElementType), MaxTime);
				}

				break;
		}
	}

	/// <summary>
	/// Whether this can apply a debuff. By default, returns <paramref name="defaultPercent"/>, which is a random chance determined by <see cref="GetDebuffChance(float)"/>.<br/>
	/// <b><see cref="ElementType.Fire"/>:</b> Only on crits.
	/// </summary>
	/// <param name="info"></param>
	/// <param name="defaultPercent"></param>
	/// <returns></returns>
	internal bool CanDebuff(NPC.HitInfo info, bool defaultPercent)
	{
		return ElementType switch
		{
			ElementType.Fire => info.Crit,
			_ => defaultPercent,
		};
	}

	/// <summary> 
	/// Get the chance of applying the associated debuff for the given damage type. 
	/// </summary>
	public static float GetDebuffChance(float damagePercent)
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


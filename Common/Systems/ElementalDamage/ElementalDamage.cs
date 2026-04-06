using PathOfTerraria.Common.NPCs.AilmentHelpers;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.NPCCritFunctionality;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Passives;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

#nullable enable

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
	/// Whether this object has any non-zero damage-related values. If it doesn't, this struct is "default" and shouldn't be synced or used for further functionality.
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
	/// The debuff type to apply. Functionality is in <see cref="ApplyBuff(ElementType, Entity, Entity, int)"/>, which may not call this method. 
	/// </summary>
	public static int GetBuffType(ElementType type)
	{
		return type switch
		{
			ElementType.Fire => ModContent.BuffType<IgnitedDebuff>(),
			ElementType.Cold => ModContent.BuffType<FreezeDebuff>(),
			ElementType.Lightning => ModContent.BuffType<ShockDebuff>(),
			_ => 0
		};
	}

	public static void ApplyBuff(ElementType elementType, Entity attacker, Entity entity, int elementalDamageDealt, bool isCrit)
	{
		switch (elementType)
		{
			case ElementType.Fire:
				IgnitedDebuff.ApplyTo(attacker, entity, (int)(elementalDamageDealt * 0.9f));
				break;

			case ElementType.Cold:

				if (isCrit)
				{
					TryApplyFreeze(elementType, attacker, entity, elementalDamageDealt);
				}

				if (Main.rand.NextFloat() < AilmentUtils.GetEntityAilmentThreshold(entity, elementalDamageDealt))
				{
					if (entity is Player chilledPlr)
					{
						chilledPlr.AddBuff(BuffID.Chilled, 4 * 60);
					}
					else if (entity is NPC chilledNPC)
					{
						chilledNPC.AddBuff(BuffID.Chilled, 4 * 60);
					}
				}

				break;

			case ElementType.Lightning:
				ShockDebuff.Apply(attacker, entity, elementalDamageDealt);

				break;

			default:
				const int MaxTime = 5 * 40;

				if (entity is NPC npc)
				{
					npc.AddBuff(GetBuffType(elementType), MaxTime);
				}
				else if (entity is Player hurtPlayer)
				{
					hurtPlayer.AddBuff(GetBuffType(elementType), MaxTime);
				}

				break;
		}
	}

	private static void TryApplyFreeze(ElementType elementType, Entity attacker, Entity entity, int elementalDamageDealt)
	{
		if (entity is NPC frozenNPC)
		{
			float baseEffectiveness = 3.6f * AilmentUtils.GetNPCAilmentThreshold(frozenNPC, elementalDamageDealt);
			float duration = attacker is Player p ? p.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.FreezeEffectiveness.ApplyTo(baseEffectiveness) : baseEffectiveness;

			if (duration > 0.3f)
			{
				frozenNPC.AddBuff(GetBuffType(elementType), (int)(duration * 60));
				frozenNPC.GetGlobalNPC<FreezeNPC>().Frozen = true;
				FreezeNPC.ConvertFrozenGore(frozenNPC);
			}
		}
		else if (entity is Player frozenPlayer)
		{
			float baseEffectiveness = 3.6f * AilmentUtils.GetPlayerAilmentThreshold(frozenPlayer, elementalDamageDealt);
			float duration = attacker is Player p ? p.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.FreezeEffectiveness.ApplyTo(baseEffectiveness) : baseEffectiveness;

			if (duration > 0.3f)
			{
				frozenPlayer.AddBuff(BuffID.Frozen, (int)(duration * 60));
			}
		}
	}

	/// <summary>
	/// Whether this can apply a debuff. By default, returns <paramref name="defaultPercent"/>, which is a random chance determined by <see cref="GetDebuffChance(float)"/>.<br/>
	/// <b><see cref="ElementType.Fire"/>:</b> Only on crits.
	/// </summary>
	/// <param name="info"></param>
	/// <param name="defaultPercent"></param>
	/// <returns></returns>
	internal static bool CanDebuff(ElementType type, Entity victim, Entity attacker, HitInfoContainer info, float defaultPercent)
	{
		if (attacker is Player player)
		{
			NPC.HitInfo hit = info.NPCHurt!.Value;

			return type switch
			{
				ElementType.Fire => Main.rand.NextFloat() < AilmentUtils.GetEntityAilmentThreshold(victim, info.DamageDealt),
				ElementType.Cold => true, // Since the Cold element has both Freeze and Chilled, always return true so it can check both individually
				ElementType.Chaos => false,
				ElementType.Lightning => hit.Crit || defaultPercent + player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<ShockChancePassive>() / 100f > Main.rand.NextFloat(),
				_ => defaultPercent > Main.rand.NextFloat(),
			};
		}
		else if (info.PlayerHurt is { } hurt)
		{
			bool isCrit = CriticalStrikeNPC.CurrentlyCritting;

			return type switch
			{
				ElementType.Fire => Main.rand.NextFloat() < AilmentUtils.GetEntityAilmentThreshold(victim, info.DamageDealt),
				ElementType.Cold => true, // Since the Cold element has both Freeze and Chilled, always return true so it can check both individually
				ElementType.Chaos => false,
				ElementType.Lightning => isCrit || defaultPercent > Main.rand.NextFloat(),
				_ => defaultPercent > Main.rand.NextFloat(),
			};
		}

#if DEBUG
		Main.NewText("ElementalDamage.CanDebuff: Unsure how this happened. Tell Gabe this appeared.");
#endif

		return false;
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
			return 0.5f;
		}
		// Linearly increase chance from 0% -> 20% as damage goes from 2% -> 50% of max health
		else
		{
			return damagePercent * (0.5f / 0.5f);
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


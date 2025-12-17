using PathOfTerraria.Utilities;
using System.Linq;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

public class ElementalPlayer : ModPlayer
{
	public ElementalContainer Container = new();

	// TODO: could be a ModConfig toggle
	public static bool DebugMessages => false;

	/// <summary>
	/// True when <see cref="ApplyElementalDamage(Player, NPC, int, ElementType, int, int, bool)"/> is running. Useful for stopping recursion.
	/// </summary>
	internal static bool ApplyingElementalDamage = false;

	public override void ResetEffects()
	{
		Container.Reset(true);
	}

	/// <summary>
	/// Defines a quick way for this player to deal 'exclusively' elemental damage quickly.<br/>
	/// This works by temporarily replacing the player's <see cref="Container"/> with a container which only contains the element given with 100% conversion
	/// and <paramref name="flatDamage"/> bonus damage, then striking the NPC.<br/>
	/// Other elements cannot proc, but the given element's multiplier is considered.
	/// </summary>
	public static void ApplyElementalDamage(Player player, NPC npc, int damage, ElementType type, int flatDamage = 0, int hitDirection = 0, bool damageVariation = false)
	{
		ElementalPlayer elePlr = player.GetModPlayer<ElementalPlayer>();
		ref ElementalContainer actualContainer = ref elePlr.Container;
		ElementalContainer container = actualContainer;

		actualContainer = new();
		ref ElementalDamage damageModifier = ref actualContainer[type].DamageModifier;
		damageModifier = damageModifier.ApplyOverride(flatDamage, 1);
		actualContainer[type].Multiplier = container[type].Multiplier;

		using var _ = ValueOverride.Create(ref ApplyingElementalDamage, true);

		player.ApplyDamageToNPC(npc, damage, 0, hitDirection, damageVariation: damageVariation);

		actualContainer = container;
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

	public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			MultipliableFloat throwaway = default;
			ElementModifyDamage(Container, elemNPC.Container, ref throwaway, ref modifiers.FinalDamage, true, item);
		}
	}

	public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC) && proj.TryGetGlobalProjectile(out ElementalProjectile elemProj))
		{
			MultipliableFloat throwaway = default;
			Item item = elemProj.SourceItem == -1 ? null : ContentSamples.ItemsByType[elemProj.SourceItem];
			ElementModifyDamage(elemProj.Container, elemNPC.Container, ref throwaway, ref modifiers.FinalDamage, true, item);
		}
	}

	private static void ElementModifyDamage(ElementalContainer container, ElementalContainer other, ref MultipliableFloat preDefenseMultiplier, ref StatModifier sourceDamage, 
		bool skipPreDefense = false, Item item = null)
	{
		// Calculate base weapon elemental strength and additional conversion separately
		float baseElementalStrength = 0f;
		float additionalConversion = 0f;
		
		foreach (ElementInstance element in container)
		{
			float weaponStrength = (item is null ? 0 : ElementalWeaponSets.GetElementStrength(item.type, element.Type)) * (1 - other[element.Type].Resistance);
			float playerConversion = element.GetTotalConversion(other);
			
			baseElementalStrength += weaponStrength;
			additionalConversion += playerConversion;
		}
		
		// Cap base elemental strength at 1.0 (can't be more than 100% elemental)
		baseElementalStrength = MathF.Min(baseElementalStrength, 1f);
	
		float totalConversion = (baseElementalStrength + additionalConversion);
		
		// Apply multiplier to original damage BEFORE DEFENSE, by each element conversion percent (accounting for resistance) 
		float totalMultiplier = MathF.Max(0f, MathF.Max(totalConversion, baseElementalStrength));

		// Add in multipliers
		foreach (ElementInstance element in container)
		{
			float conv = MathF.Min(element.GetTotalConversion(other) + (item is null ? 0 : ElementalWeaponSets.GetElementStrength(item.type, element.Type)), 1);
			float bonus = (conv * element.Multiplier) - conv;
			totalMultiplier += bonus * (1 - Math.Abs(other[element.Type].Resistance));
		}

		if (!skipPreDefense)
		{
			// Adds X% damage to the hit, depending on the total conversion multiplier, before defense - only applies to players
			preDefenseMultiplier *= 1 + totalMultiplier;
		}
		else
		{
			// Adds X% final damage to the hit, depending on the total conversion multiplier - only applies to NPCs
			sourceDamage += totalMultiplier;
		}
		
		// Apply flat extra damage (accounting for resistance)
		foreach (ElementInstance element in container)
		{
			sourceDamage.Flat += element.GetFlatDamage(other) * element.Multiplier;
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

	/// <inheritdoc cref="GetConversionMultiplier(ElementInstance, Item, float)"/>
	public static float GetConversionMultiplier(ElementInstance instance, Item item, ElementalContainer other)
	{
		return GetConversionMultiplier(instance, item, other[instance.Type].Resistance);
	}

	/// <summary>
	/// Gets the conversion multiplier for the given element and optionally item.
	/// </summary>
	/// <returns>The actual conversion multiplier used.</returns>
	public static float GetConversionMultiplier(ElementInstance instance, Item item, float resistance)
	{
		return instance.GetTotalConversion(resistance) + (item is null ? 0 : ElementalWeaponSets.GetElementStrength(item.type, instance.Type));
	}

	public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
	{
		if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj))
		{
			ElementOnHit(Player, elemProj.Container, Container, hurtInfo.Damage, null);
		}
	}

	public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementOnHit(Player, elemNPC.Container, Container, hurtInfo.Damage, null);
		}
	}

	public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementOnHit(target, Container, elemNPC.Container, hit.Damage, hit, item, Player);
		}
	}

	public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			Item item = null;

			if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj) && elemProj.SourceItem > ItemID.None)
			{
				item = new(elemProj.SourceItem);
			}

			ElementOnHit(target, Container, elemNPC.Container, hit.Damage, hit, item, Player);
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (ApplyingElementalDamage && target.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementOnHit(target, Container, elemNPC.Container, hit.Damage, hit, null, Player);
		}
	}

	private static void ElementOnHit(Entity target, ElementalContainer container, ElementalContainer other, int finalDamage, NPC.HitInfo? optionalHitInfo, Item item = null, 
		Player player = null)
	{
		// Elemental damage done & debuffs
		foreach (ElementInstance element in container)
		{
			// Get total conversion for this element (and item if applicable)
			float totalConversion = GetConversionMultiplier(element, item, other);
			
			float baseWeaponConversion = item?.type > ItemID.None ? ElementalWeaponSets.GetElementStrength(item.type, element.Type) : 0f;
			float addedConversion = baseWeaponConversion + element.GetTotalConversion(0);

			// Calculate additional conversion damage and flat damage bonus - skip for base elements as it's already in main hit
			int conversionDamage = (int)(finalDamage * addedConversion);
			int flatDamage = (int)element.GetFlatDamage(other);

			// Total additional damage for this element
			int totalAdditionalDamage = conversionDamage + flatDamage;

			// Print debug message if enabled & there's anything to show
			if (DebugMessages && totalAdditionalDamage > 0)
			{
				Main.NewText($"  {element.Type}:        {totalAdditionalDamage}");
			}

			NPC npc = null;

			if (target is NPC checkNpc)
			{
				npc = checkNpc;
			}

			// Debuff applications
			if (optionalHitInfo is { } hitInfo && player is not null)
			{
				if (npc is not null)
				{
					ElementalPlayerHooks.ElementalOnHitNPC(player, false, element, npc, container, other, finalDamage, hitInfo, item);
				}

				// If theres any conversion being done on either base elemental or added elemental damage
				bool hasElement = totalAdditionalDamage > 0;

				if (hasElement)
				{
					// Calculate total elemental damage for debuff purposes (includes base weapon damage)
					int totalElementalDamageForDebuff = (int)(finalDamage * totalConversion) + flatDamage;
					TryAddElementBuff(player, target, element.DamageModifier.ElementType, totalElementalDamageForDebuff, optionalHitInfo.Value);
				}

				if (npc is not null)
				{
					ElementalPlayerHooks.ElementalOnHitNPC(player, true, element, npc, container, other, finalDamage, hitInfo, item);
				}
			}
		}

		if (player is Player plr && target is NPC postNpc && optionalHitInfo.HasValue)
		{
			ElementalPlayerHooks.PostElementalHit(plr, postNpc, container, other, finalDamage, optionalHitInfo.Value, item);
		}
	}

	/// <summary>
	/// Used to apply elemental debuffs. This can be called manually from any <see cref="ModPlayer.OnHitNPC(NPC, NPC.HitInfo, int)"/>.
	/// </summary>
	/// <exception cref="ArgumentException"></exception>
	internal static bool TryAddElementBuff(Player player, Entity target, ElementType elementType, int elementalDamageDone, NPC.HitInfo hitInfo)
	{
		int lifeMax = target switch
		{
			NPC npc => npc.lifeMax,
			Player playerTarget => playerTarget.statLifeMax2,
			_ => throw new ArgumentException("target should be an NPC or Player!")
		};

		float chance = ElementalDamage.GetDebuffChance((float)elementalDamageDone / lifeMax);

		if (DebugMessages)
		{
			Main.NewText($"[DEBUG] Chance to debuff for {elementType}: {chance * 100:0.##}%");
		}

		if (elementalDamageDone > 0 && ElementalDamage.CanDebuff(elementType, target, player, hitInfo, chance))
		{
			ElementalDamage.ApplyBuff(elementType, player, target, elementalDamageDone);

			if (DebugMessages)
			{
				Main.NewText($"[DEBUG] Debuff for {elementType} applied!");
			}

			return true;
		}

		return false;
	}
}
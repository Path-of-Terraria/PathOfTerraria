using System.Linq;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

public class ElementalPlayer : ModPlayer
{
	public ElementalContainer Container = new();

	// TODO: could be a ModConfig toggle
	public static bool DebugMessages => false;
	
	private static bool isProcessingElementalDamage = false;

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

	public float GetConversionMultiplier(ElementType type, Item item, ElementalContainer other)
	{
		return MathF.Min(Container[ElementType.Fire].GetTotalConversion(other), ElementalWeaponSets.GetElementStrength(item.type, type));
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

	public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementOnHit(target, Container, elemNPC.Container, hit.SourceDamage, hit.Damage, hit, item);
		}
	}

	public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			var item = new Item();
			if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj) && elemProj.SourceItem > ItemID.None)
			{
				item.SetDefaults(elemProj.SourceItem);
			}
			ElementOnHit(target, Container, elemNPC.Container, hit.SourceDamage, hit.Damage, hit, item);
		}
	}

	private static void ElementOnHit(Entity target, ElementalContainer container, ElementalContainer other, float sourceDamage, int finalDamage, NPC.HitInfo? optionalHitInfo, 
		Item item = null)
	{
		// Prevent recursion (I commented this out and it seems to not be doing anything so im not sure if its needed...) Gabe?
		if (isProcessingElementalDamage)
		{
			return;
		}

		// Elemental damage done & debuffs
		foreach (ElementInstance element in container)
		{
			// Get total conversion for this element
			float totalConversion = GetTotalConversion(element);

			// Check if this is a base weapon element. True if it IS a part of the base element damage.
			bool isBaseElement = ElementalWeaponSets.GetElementStrength(item?.type ?? ItemID.None, element.Type) > 0f;

			// Get added conversion (only player/gear bonuses, excludes base weapon)
			float addedConversion = element.DamageModifier.DamageConversion;

			// Calculate additional conversion damage and  flat damage bonus - skip for base elements as it's already in main hit
			int conversionDamage = 0;
			int flatDamage = 0;
			// We only want to have the base damage be stuff be calculated in ModifyHitNPCWithItem & Projectile below, to ensure it's not ANOTHER hit.
			if (!isBaseElement)
			{
				flatDamage = (int)element.GetFlatDamage(other);
			}

			conversionDamage = (int)(finalDamage * addedConversion);

			// Total additional damage for this element
			int totalAdditionalDamage = conversionDamage + flatDamage;

			if (DebugMessages && totalAdditionalDamage > 0)
			{
				Main.NewText($"  {element.Type}:        {totalAdditionalDamage}");
			}

			// Apply additional elemental damage via StrikeNPC if any
			if (totalAdditionalDamage > 0 && target is NPC npc)
			{
				isProcessingElementalDamage = true;
	
				try
				{
					float knockback = 0f; // Dont want multiple instances of knockback
					bool crit = false; // TODO: Crit damage is applying properly. However, the text isnt orange as it's not counted as an actual crit. 
		
					var additionalHitInfo = npc.CalculateHitInfo(totalAdditionalDamage, 0, crit, knockback);
					//In the future, we can add some kind of "delayed" echo effect to this Strike. 
					npc.StrikeNPC(additionalHitInfo);

					if (DebugMessages)
					{
						Main.NewText($"Applied {totalAdditionalDamage} additional {element.Type} damage ({conversionDamage} conversion + {flatDamage} flat) via StrikeNPC");
					}
				}
				finally
				{
					isProcessingElementalDamage = false;
				}
			}

			// Debuff applications
			if (optionalHitInfo is not null)
			{
				// If theres any conversion being done on either base elemental or added/echod elemental damage
				bool hasElement = (totalConversion > 0f) || element.DamageModifier.HasValues;

				if (hasElement)
				{
					// Calculate total elemental damage for debuff purposes (includes base weapon damage)
					int totalElementalDamageForDebuff = (int)(finalDamage * totalConversion) + flatDamage;
					TryAddElementBuff(target, element.DamageModifier, totalElementalDamageForDebuff, optionalHitInfo.Value);
				}
			}
		}
		
		float GetTotalConversion(ElementInstance instance)
		{
			return item is null ? 
				instance.GetTotalConversion(other) :
				MathF.Min(instance.GetTotalConversion(other) + ElementalWeaponSets.GetElementStrength(item.type, instance.Type), 1);
		}
	}

	private static bool TryAddElementBuff(Entity target, ElementalDamage damage, int elementalDamageDone, NPC.HitInfo hitInfo)
	{
		int lifeMax = target switch
		{
			NPC npc => npc.lifeMax,
			Player player => player.statLifeMax2,
			_ => throw new ArgumentException("target should be an NPC or Player!")
		};

		float chance = ElementalDamage.GetDebuffChance((float)elementalDamageDone / lifeMax);
		if (DebugMessages)
		{
			Main.NewText($"[DEBUG] Chance to debuff for {damage.ElementType}: {chance * 100:0.##}%");
		}

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
		
	public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			ElementModifyNPCDamage(Container, elemNPC.Container, ref modifiers.SourceDamage, item);
		}
	}

	public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (target.TryGetGlobalNPC(out ElementalNPC elemNPC))
		{
			Item item = null;
			if (proj.TryGetGlobalProjectile(out ElementalProjectile elemProj) && elemProj.SourceItem > ItemID.None)
			{
				item = new Item();
				item.SetDefaults(elemProj.SourceItem);
			}
			ElementModifyNPCDamage(Container, elemNPC.Container, ref modifiers.SourceDamage, item);
		}
	}

	//This modifies the base elemental hit
	private void ElementModifyNPCDamage(ElementalContainer attackerContainer, ElementalContainer targetContainer, ref StatModifier sourceDamage, Item item = null)
	{
		// Add flat damage and added conversion for matching base weapon elements to source damage
		foreach (ElementInstance element in attackerContainer)
		{
			// Check if this weapon has this element as a base element
			float baseWeaponConversion = item?.type > ItemID.None ? ElementalWeaponSets.GetElementStrength(item.type, element.Type) : 0f;
			bool isBaseElement = baseWeaponConversion > 0f;
	
			if (isBaseElement)
			{
				// For base elements, add flat damage to source so it's part of the main hit
				float flatDamage = element.GetFlatDamage(targetContainer);
				sourceDamage.Flat += flatDamage;

				if (DebugMessages && (flatDamage > 0))
				{
					Main.NewText($"[DEBUG] Base {element.Type}: +{flatDamage} flat to main hit");
				}
			}
		}
	}


}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

public class ElementInstance(ElementType type, bool isGeneric)
{
	public readonly ElementType Type = type;

	/// <summary>
	/// Determines if this element is "generic", i.e. it's included in "All elemental" affixes or effects.
	/// </summary>
	public readonly bool IsGeneric = isGeneric;

	public LocalizedText ElementDisplayName => Language.GetOrRegister("Mods.PathOfTerraria.Misc.Element." + Type + ".Name");

	/// <summary>
	/// <inheritdoc cref="GetTotalConversion(float)"/>
	/// </summary>
	/// <param name="container">The container to get the resistance of which corresponds to the current <see cref="ElementInstance"/>'s <see cref="Type"/>.</param>
	/// <returns><inheritdoc cref="GetTotalConversion(float)"/></returns>
	public float GetTotalConversion(ElementalContainer container)
	{
		return GetTotalConversion(container[Type].Resistance);
	}

	/// <summary>
	/// <inheritdoc cref="GetTotalConversion(float)"/>
	/// </summary>
	/// <param name="instance">The element to get the resistance of.</param>
	/// <returns><inheritdoc cref="GetTotalConversion(float)"/></returns>
	public float GetTotalConversion(ElementInstance instance)
	{
		return GetTotalConversion(instance.Resistance);
	}

	/// <summary>
	/// How much of damage is being converted by this instance, taking into account the victim's resistance.
	/// </summary>
	/// <param name="resistance">% of resistance of this element the victim has.</param>
	/// <returns>Final conversion %, accounting for resistance.</returns>
	public float GetTotalConversion(float resistance)
	{
		return DamageModifier.DamageConversion * Math.Abs(1f - resistance);
	}

	/// <summary>
	/// <inheritdoc cref="GetFlatDamage(float)"/>
	/// </summary>
	/// <param name="container">The container to get the flat damage of which corresponds to the current <see cref="ElementInstance"/>'s <see cref="Type"/>.</param>
	/// <returns><inheritdoc cref="GetFlatDamage(float)"/></returns>
	public float GetFlatDamage(ElementalContainer container)
	{
		return GetFlatDamage(container[Type].Resistance);
	}

	/// <summary>
	/// <inheritdoc cref="GetFlatDamage(float)"/>
	/// </summary>
	/// <param name="instance">The element to get the flat damage of.</param>
	/// <returns><inheritdoc cref="GetFlatDamage(float)"/></returns>
	public float GetFlatDamage(ElementInstance instance)
	{
		return GetFlatDamage(instance.Resistance);
	}

	/// <summary>
	/// How much damage is flatly added to the hit, taking in the victim's resistance.
	/// </summary>
	/// <returns>The total flat damage done, accounting for resistance.</returns>
	public float GetFlatDamage(float resistance)
	{
		return DamageModifier.DamageBonus * (1f - resistance);
	}

	public float Resistance
	{
		get => _resistance;
		set => _resistance = MathHelper.Min(value, 0.75f);
	}

	private float _resistance = 0;

	public ElementalDamage DamageModifier = new(type);

	/// <summary>
	/// Multiplies damage done.
	/// </summary>
	public float Multiplier = 1;

	public void Reset(bool resetModifiers)
	{
		Multiplier = 1;
		Resistance = 0;

		if (resetModifiers)
		{
			DamageModifier = DamageModifier.ApplyOverride(0, 0);
		}
	}

	/// <summary>
	/// Writes this <see cref="ElementInstance"/> to the writers.
	/// </summary>
	public void WriteTo(BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		bitWriter.WriteBit(DamageModifier.HasValues);

		if (DamageModifier.HasValues)
		{
			DamageModifier.Write(binaryWriter);
		}
	}

	/// <summary>
	/// Updates this <see cref="ElementInstance"/> to use the values passed in by another <see cref="WriteTo(BitWriter, BinaryWriter)"/> call.
	/// </summary>
	public void ReadFrom(BitReader bitReader, BinaryReader binaryReader)
	{
		bool hasValue = bitReader.ReadBit();

		if (hasValue)
		{
			DamageModifier = ElementalDamage.Read(binaryReader);
		}
	}

	public override string ToString()
	{
		return $"{Type}: Conv: {GetTotalConversion(0)} Flat: {GetFlatDamage(0)} Mult: {Multiplier}";
	}
}

/// <summary>
/// Holds all relevant elemental information for an entity, such as damage boosts and damage resistance.<br/>
/// These can be enumerated, returning every <see cref="ElementInstance"/> in the container.
/// </summary>
public class ElementalContainer : IEnumerable<ElementInstance>
{
	/// <summary>
	/// Returns the <see cref="ElementInstance"/> of the given type from this container.
	/// </summary>
	public ElementInstance this[ElementType type] => Instances[type];

	public readonly Dictionary<ElementType, ElementInstance> Instances;

	/// <summary>
	/// Percent of damage-to-be-done that will be converted to an elemental damage type.
	/// </summary>
	public float TotalConversion
	{
		get
		{
			float total = 0f;

			foreach (ElementInstance instance in this)
			{
				total += instance.DamageModifier.DamageConversion;
			}

			return MathHelper.Clamp(total, 0f, 1f);
		}
	}

	public ElementalContainer()
	{
		Instances = [];

		AddNewElement(ElementType.Fire);
		AddNewElement(ElementType.Cold);
		AddNewElement(ElementType.Lightning);
		AddNewElement(ElementType.Chaos, false);

		void AddNewElement(ElementType type, bool generic = true)
		{
			Instances.Add(type, new ElementInstance(type, generic));
			_ = Instances[type].ElementDisplayName; // Autoregister the element name
		}
	}

	/// <summary>
	/// Shorthand for setting <see cref="ElementalDamage"/> and <see cref="ElementInstance.Multiplier"/>. 
	/// Also see <see cref="AddElementalValues(ValueTuple{ElementType, int, float}[])"/>.
	/// </summary>
	public void AddElementalValues(params (ElementType type, int add, float conv, float multiplier)[] values)
	{
		foreach ((ElementType type, int add, float conv, float multiplier) in values)
		{
			ref ElementalDamage damageModifier = ref this[type].DamageModifier;
			damageModifier = damageModifier.AddModifiers(add, conv);
			this[type].Multiplier += multiplier;
		}
	}

	/// <summary>
	/// Shorthand for setting <see cref="ElementalContainer"/>'s <see cref="ElementalDamage"/> values. 
	/// Also see <see cref="AddElementalValues(ValueTuple{ElementType, int, float, float}[])"/>.
	/// </summary>
	public void AddElementalValues(params (ElementType type, int add, float conv)[] values)
	{
		foreach ((ElementType type, int add, float conv) in values)
		{
			ref ElementalDamage damageModifier = ref this[type].DamageModifier;
			damageModifier = damageModifier.AddModifiers(add, conv);
		}
	}

	/// <summary>
	/// Resets the modifiers on this instance for use in something like <see cref="ModPlayer.ResetEffects"/>.
	/// </summary>
	/// <param name="resetModifiers">Whether the elemental modifiers (such as <see cref="LightningDamageModifier"/>) should be reset. NPCs shouldn't reset them.</param>
	public void Reset(bool resetModifiers)
	{
		foreach (ElementInstance instance in this)
		{
			instance.Reset(resetModifiers);
		}
	}

	/// <summary>
	/// Writes this <see cref="ElementalContainer"/> to the given bit/binary writers.
	/// </summary>
	public void WriteTo(BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		foreach (ElementInstance instance in this)
		{
			instance.WriteTo(bitWriter, binaryWriter);
		}
	}

	/// <summary>
	/// Updates this container to use the values passed in by another <see cref="WriteTo(BitWriter, BinaryWriter)"/> call.
	/// </summary>
	public void ReadFrom(BitReader bitReader, BinaryReader binaryReader)
	{
		foreach (ElementInstance instance in this)
		{
			instance.ReadFrom(bitReader, binaryReader);
		}
	}

	public ElementalContainer Clone()
	{
		return (ElementalContainer)MemberwiseClone();
	}

	public IEnumerator<ElementInstance> GetEnumerator()
	{
		return Instances.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

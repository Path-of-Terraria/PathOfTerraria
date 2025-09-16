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
	/// How much of damage is being converted by this instance, taking in <see cref="Resistance"/>.
	/// </summary>
	public float TotalConversion => DamageModifier.DamageConversion * (1f - Resistance);

	/// <summary>
	/// How much damage is flatly added to the hit, taking in <see cref="Resistance"/>.
	/// </summary>
	public float TotalFlatDamage => DamageModifier.DamageBonus * (1f - Resistance);

	public float Resistance
	{
		get => _resistance;
		set => _resistance = MathHelper.Clamp(value, 0f, 0.75f);
	}

	private float _resistance = 0;

	public ElementalDamage DamageModifier = new(type);

	// Copied from original implementation, where this also did nothing
	public float Multiplier = 0;

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

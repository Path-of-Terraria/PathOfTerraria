using System.IO;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

/// <summary>
/// Holds all relevant elemental information for an entity, such as damage boosts and damage resistance.
/// </summary>
public class ElementalContainer
{
	private float _fireResistance;
	private float _coldResistance;
	private float _lightningResistance;

	/// <summary>
	/// Controls the scaling damage decrease from fire damage. Clamps at 75%.
	/// </summary>
	public float FireResistance
	{
		get => _fireResistance;
		set => _fireResistance = MathHelper.Clamp(value, 0f, 0.75f);
	}

	/// <summary>
	/// Controls the scaling damage decrease from cold damage. Clamps at 75%.
	/// </summary>
	public float ColdResistance
	{
		get => _coldResistance;
		set => _coldResistance = MathHelper.Clamp(value, 0f, 0.75f);
	}

	/// <summary>
	/// Controls the scaling damage decrease from lightning damage. Clamps at 75%.
	/// </summary>
	public float LightningResistance
	{
		get => _lightningResistance;
		set => _lightningResistance = MathHelper.Clamp(value, 0f, 0.75f);
	}

	/// <summary>
	/// Percent of damage-to-be-done that will be converted to an elemental damage type.
	/// </summary>
	public float TotalConversion => MathHelper.Clamp(FireDamageModifier.DamageConversion + ColdDamageModifier.DamageConversion + LightningDamageModifier.DamageConversion, 0f, 1f);

	public ElementalDamage FireDamageModifier = new(ElementType.Fire);
	public ElementalDamage ColdDamageModifier = new(ElementType.Cold);
	public ElementalDamage LightningDamageModifier = new(ElementType.Lightning);

	// I don't know what these are for? They're used in some passives, but they don't actually do anything...
	public float FireMultiplier = 1f;
	public float ColdMultiplier = 1f;
	public float LightningMultiplier = 1f;

	/// <summary>
	/// Resets the modifiers on this instance for use in something like <see cref="ModPlayer.ResetEffects"/>.
	/// </summary>
	/// <param name="resetModifiers">Whether the elemental modifiers (such as <see cref="LightningDamageModifier"/>) should be reset. NPCs shouldn't reset them.</param>
	public void Reset(bool resetModifiers)
	{
		FireMultiplier = ColdMultiplier = LightningMultiplier = 1;
		FireResistance = ColdResistance = LightningResistance = 0;

		if (resetModifiers)
		{
			FireDamageModifier.ApplyOverride(0, 0);
			ColdDamageModifier.ApplyOverride(0, 0);
			LightningDamageModifier.ApplyOverride(0, 0);
		}
	}

	/// <summary>
	/// Writes this <see cref="ElementalContainer"/> to the given bit/binary writers.
	/// </summary>
	public void WriteTo(BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		bitWriter.WriteBit(FireDamageModifier.HasValues);
		bitWriter.WriteBit(ColdDamageModifier.HasValues);
		bitWriter.WriteBit(LightningDamageModifier.HasValues);

		if (FireDamageModifier.HasValues)
		{
			FireDamageModifier.Write(binaryWriter);
		}

		if (ColdDamageModifier.HasValues)
		{
			ColdDamageModifier.Write(binaryWriter);
		}

		if (LightningDamageModifier.HasValues)
		{
			LightningDamageModifier.Write(binaryWriter);
		}
	}

	/// <summary>
	/// Updates this container to use the values passed in by another <see cref="WriteTo(BitWriter, BinaryWriter)"/> call.
	/// </summary>
	public void ReadFrom(BitReader bitReader, BinaryReader binaryReader)
	{
		bool fire = bitReader.ReadBit();
		bool cold = bitReader.ReadBit();
		bool lightning = bitReader.ReadBit();

		if (fire)
		{
			FireDamageModifier = ElementalDamage.Read(binaryReader);
		}

		if (cold)
		{
			ColdDamageModifier = ElementalDamage.Read(binaryReader);
		}

		if (lightning)
		{
			LightningDamageModifier = ElementalDamage.Read(binaryReader);
		}
	}
}

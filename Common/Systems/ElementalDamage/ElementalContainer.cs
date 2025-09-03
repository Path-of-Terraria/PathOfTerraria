using System.ComponentModel;
using System.IO;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

public class ElementalContainer
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

	/// <summary>
	/// % of damage-to-be-done that will be converted to an elemental damage type.
	/// </summary>
	public float TotalConversion => MathHelper.Clamp(FireDamageModifier.DamageConversion + ColdDamageModifier.DamageConversion + LightningDamageModifier.DamageConversion, 0f, 1f);

	public ElementalDamage FireDamageModifier = new(ElementType.Fire);
	public ElementalDamage ColdDamageModifier = new(ElementType.Cold);
	public ElementalDamage LightningDamageModifier = new(ElementType.Lightning);

	// Kept from unused code. Not sure their purpose?
	public float FireMultiplier = 1f;
	public float ColdMultiplier = 1f;
	public float LightningMultiplier = 1f;

	public void Reset(bool resetModifiers)
	{
		FireMultiplier = ColdMultiplier = LightningMultiplier = 1;
		FireResistance = ColdResistance = LightningResistance = 0;

		if (resetModifiers)
		{
			FireDamageModifier = new(ElementType.Fire);
			ColdDamageModifier = new(ElementType.Cold);
			LightningDamageModifier = new(ElementType.Lightning);
		}
	}

	/// <summary>
	/// Writes this <see cref="ElementalContainer"/> to the given bit/binary writers.
	/// </summary>
	/// <param name="bitWriter"></param>
	/// <param name="binaryWriter"></param>
	public void WriteTo(BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		bitWriter.WriteBit(FireDamageModifier.Valid);
		bitWriter.WriteBit(ColdDamageModifier.Valid);
		bitWriter.WriteBit(LightningDamageModifier.Valid);

		if (FireDamageModifier.Valid)
		{
			FireDamageModifier.Write(binaryWriter);
		}

		if (ColdDamageModifier.Valid)
		{
			ColdDamageModifier.Write(binaryWriter);
		}

		if (LightningDamageModifier.Valid)
		{
			LightningDamageModifier.Write(binaryWriter);
		}
	}

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

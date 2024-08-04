using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.Affixes;

internal class AffixPlayer : ModPlayer
{
	public Dictionary<string, float> AffixStrengthByFullName = [];

	public override void ResetEffects()
	{
		AffixStrengthByFullName.Clear();
	}

	public void AddStrength(string name, float strength)
	{
		if (AffixStrengthByFullName.TryGetValue(name, out float value))
		{
			AffixStrengthByFullName[name] = value + strength;
		}
		else
		{
			AffixStrengthByFullName.Add(name, strength);
		}
	}

	public float StrengthOf<T>() where T : Affix
	{
		return StrengthOf(typeof(T).AssemblyQualifiedName);
	}

	public float StrengthOf(string name)
	{
		return AffixStrengthByFullName.TryGetValue(name, out float value) ? value : 0;
	}
}

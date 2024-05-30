using PathOfTerraria.Content.Items.Gear;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Affixes;

internal abstract class Affix
{
	protected float _minValue = 0f;
	protected float _maxValue = 1f;
	protected float _externalMultiplier = 1f;
	protected float Value = 1f;
	// to a certain degree, none of the above is useable by the MobAffix...

	public virtual void Roll()
	{
		Value = Main.rand.Next((int)(_minValue * 10), (int)(_maxValue * 10)) / 10f;
	}

	public void Save(TagCompound tag)
	{
		tag["type"] = GetType().FullName;
		tag["externalMultiplier"] = _externalMultiplier;
		tag["value"] = Value;
	}

	public void Load(TagCompound tag)
	{
		_externalMultiplier = tag.GetFloat("externalMultiplier");
		Value = tag.GetFloat("value");
	}

	public static Affix CreateAffix<T>(float externalMultiplier = 1, float value = -1)
	{
		var instance = (Affix)Activator.CreateInstance(typeof(T));

		instance._externalMultiplier = externalMultiplier;
		if (value == -1)
		{
			instance.Roll();
		}
		else
		{
			instance.Value = value;
		}

		return instance;
	}
	public T Clone<T>() where T : Affix
	{
		var clone = (T)Activator.CreateInstance(GetType());

		clone._minValue = _minValue;
		clone._maxValue = _maxValue;
		clone._externalMultiplier = _externalMultiplier;
		clone.Value = Value;

		return clone;
	}

	/// <summary>
	/// Used to generate a list of random affixes
	/// </summary>
	/// <param name="inputList">The list of affixes to pick from</param>
	/// <param name="count"></param>
	/// <returns></returns>
	public static List<T> GenerateAffixes<T>(List<T> inputList, int count) where T : Affix
	{
		if (inputList.Count <= count)
		{
			return inputList;
		}

		var resultList = new List<T>(count);

		for (int i = 0; i < count; i++)
		{
			int randomIndex = Main.rand.Next(0, inputList.Count);

			T newGearAffix = inputList[randomIndex].Clone<T>();
			newGearAffix.Roll();

			resultList.Add(newGearAffix);
			inputList.RemoveAt(randomIndex);
		}

		return resultList;
	}
}
internal class AffixHandler : ILoadable
{
	private static List<GearAffix> _gearAffixes;
	private static List<MapAffix> _mapAffixes;
	private static List<MobAffix> _mobAffixes;

	/// <summary>
	/// Returns a list of gear affixes that are valid for the given type. Typically used to roll affixes.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="influence"></param>
	/// <returns></returns>
	public static List<GearAffix> GetAffixes(GearType type, GearInfluence influence)
	{
		return _gearAffixes
			.Where(proto => proto.RequiredInfluence == GearInfluence.None || proto.RequiredInfluence == influence)
			.Where(proto => (type & proto.PossibleTypes) == type)
			.ToList();
	}

	/// <summary>
	/// Returns a list of map affixes that are valid for the given type. Typically used to roll affixes.
	/// </summary>
	/// <returns></returns>
	public static List<MapAffix> GetAffixes()
	{
		return _mapAffixes
			.ToList();
	}

	/// <summary>
	/// Returns a list of mob affixes that are valid for the given type. Typically used to roll affixes.
	/// </summary>
	/// <returns></returns>
	public static List<MobAffix> GetAffixes(MobRarity rarity)
	{
		return _mobAffixes
			.Where(proto => rarity >= proto.MinimumRarity)
			.Where(proto => proto.Allowed)
			.ToList();
	}

	public void Load(Mod mod)
	{
		_gearAffixes = [];
		_mapAffixes = [];
		_mobAffixes = [];

		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Affix)))
			{
				continue;
			}

			object instance = Activator.CreateInstance(type);

			if (type.IsSubclassOf(typeof(GearAffix)))
			{
				_gearAffixes.Add(instance as GearAffix);
				continue;
			}

			if (type.IsSubclassOf(typeof(MapAffix)))
			{
				_mapAffixes.Add(instance as MapAffix);
				continue;
			}

			if (type.IsSubclassOf(typeof(MobAffix)))
			{
				_mobAffixes.Add(instance as MobAffix);
				continue;
			}
		}
	}

	public void Unload()
	{
		_gearAffixes = null;
		_mapAffixes = null;
		_mobAffixes = null;
	}
}
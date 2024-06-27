using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Affixes;

public abstract class Affix
{
	public float MinValue;
	public float MaxValue = 1f;

	public float Value = 1f;
	// to a certain degree, none of the above is useable by the MobAffix...

	public virtual void Roll()
	{
		if (Value == 0)
		{
			Value = Main.rand.Next((int)(MinValue * 10), (int)(MaxValue * 10)) / 10f;	
		}
	}

	public void Save(TagCompound tag)
	{
		tag["type"] = GetType().FullName;
		tag["value"] = Value;
		tag["maxValue"] = MaxValue;
		tag["minValue"] = MinValue;
	}

	protected void Load(TagCompound tag)
	{
		Value = tag.GetFloat("value");
		MaxValue = tag.GetFloat("maxValue");
		MinValue = tag.GetFloat("minValue");
	}

	public static Affix CreateAffix<T>(float value = -1, float minValue = 0f, float maxValue = 1f)
	{
		var instance = (Affix)Activator.CreateInstance(typeof(T));

		instance.MinValue = minValue;
		instance.MaxValue = maxValue;

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

	/// <summary>
	/// Generates an affix from a tag, used on load to re-populate affixes
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static T FromTag<T>(TagCompound tag) where T : Affix
	{
		Type t = typeof(ItemAffix).Assembly.GetType(tag.GetString("type"));
		if (t is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
			return null;
		}

		var affix = (T)Activator.CreateInstance(t);

		if (affix is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
			return null;
		}

		affix.Load(tag);
		return affix;
	}

	public T Clone<T>() where T : Affix
	{
		var clone = (T)Activator.CreateInstance(GetType());

		clone.MinValue = MinValue;
		clone.MaxValue = MaxValue;
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

			T newItemAffix = inputList[randomIndex].Clone<T>();
			newItemAffix.Roll();

			resultList.Add(newItemAffix);
			inputList.RemoveAt(randomIndex);
		}

		return resultList;
	}
}

internal class AffixHandler : ILoadable
{
	private static List<ItemAffix> _itemAffixes;
	private static List<MobAffix> _mobAffixes;

	/// <summary>
	/// Returns a list of gear affixes that are valid for the given type. Typically used to roll affixes.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="influence"></param>
	/// <returns></returns>
	public static List<ItemAffix> GetAffixes(PoTItem item)
	{
		return _itemAffixes
			.Where(proto => proto.RequiredInfluence == Influence.None || proto.RequiredInfluence == item.Influence)
			.Where(proto => (item.ItemType & proto.PossibleTypes) == item.ItemType)
			.ToList();
	}

	public static List<ItemAffix> GetAffixes()
	{
		return _itemAffixes;
	}

	/// <summary>
	/// Returns a list of mob affixes that are valid for the given type. Typically used to roll affixes.
	/// </summary>
	/// <returns></returns>
	public static List<MobAffix> GetAffixes(Rarity rarity)
	{
		return _mobAffixes
			.Where(proto => rarity >= proto.MinimumRarity)
			.Where(proto => proto.Allowed)
			.ToList();
	}

	public void Load(Mod mod)
	{
		_itemAffixes = [];
		_mobAffixes = [];

		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Affix)))
			{
				continue;
			}

			object instance = Activator.CreateInstance(type);

			if (instance is ItemAffix itemAffix)
			{
				_itemAffixes.Add(itemAffix);
				continue;
			}

			if (instance is MobAffix mobAffix)
			{
				_mobAffixes.Add(mobAffix);
				continue;
			}
		}
	}

	public void Unload()
	{
		_itemAffixes = null;
		_mobAffixes = null;
	}
}
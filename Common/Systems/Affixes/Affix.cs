using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Enums;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Core;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

namespace PathOfTerraria.Common.Systems.Affixes;

public abstract class Affix : ILocalizedModType
{
	public float MinValue;
	public float MaxValue = 1f;
	public float Value = 0;
	public int Duration = 180; //3 Seconds by default
	public bool IsCorruptedAffix = false;

	public string LocalizationCategory => "Affixes";
	public Mod Mod => PoTMod.Instance; // TODO: Cross mod compat?
	public string Name => GetType().Name;
	public string FullName => Mod.Name + "/" + Name;

	// to a certain degree, none of the above is useable by the MobAffix...

	public virtual void Roll()
	{
		if (Value == 0)
		{
			Value = Main.rand.NextFloat(MinValue, MaxValue);
		}
	}

	/// <summary>
	/// Called during <see cref="AffixHandler.Load(Mod)"/> on the prototype instance for the instance, 
	/// stored in <see cref="AffixHandler._itemAffixes"/> or <see cref="AffixHandler._mobAffixes"/>.<br/>
	/// Should be used for one-time loading tasks, like subscribing to detours or adding content.
	/// </summary>
	public virtual void OnLoad()
	{
	}

	/// <summary>
	/// Called during <see cref="AffixHandler.Unload()"/> on the prototype instance for the instance, 
	/// stored in <see cref="AffixHandler._itemAffixes"/> or <see cref="AffixHandler._mobAffixes"/>.<br/>
	/// Should be used for one-time unloading tasks, like unsubscribing to detours or removing.
	/// </summary>
	public virtual void OnUnload()
	{
	}

	public void Save(TagCompound tag)
	{
		tag["type"] = GetType().FullName;
		tag["value"] = Value;
		tag["maxValue"] = MaxValue;
		tag["minValue"] = MinValue;
	}

	public void Load(TagCompound tag)
	{
		Value = tag.GetFloat("value");
		MaxValue = tag.GetFloat("maxValue");
		MinValue = tag.GetFloat("minValue");
	}

	public virtual void NetSend(BinaryWriter writer)
	{
		writer.Write(AffixHandler.IndexFromItemAffix(this));

		writer.Write(Value);
		writer.Write(MaxValue);
		writer.Write(MinValue);
	}

	public void NetReceive(BinaryReader reader)
	{
		Value = reader.ReadSingle();
		MaxValue = reader.ReadSingle();
		MinValue = reader.ReadSingle();
	}

	/// <summary>
	/// Creates an affix with the given value. If you want a range, use <see cref="CreateAffix{T}(float, float)"/>.
	/// </summary>
	/// <param name="value">The set value of the affix.</param>
	/// <typeparam name="T">The affix type to create.</typeparam>
	/// <returns>The new affix.</returns>
	public static Affix CreateAffix<T>(float value)
	{
		Affix instance = (Affix)Activator.CreateInstance(typeof(T)) ?? throw new Exception($"Could not create affix of type {typeof(T).Name}");
		instance.Value = value;
		return instance;
	}
	
	/// <summary>
	/// Creates an affix with a value between <paramref name="minValue"/> and <paramref name="maxValue"/>.
	/// </summary>
	/// <param name="minValue">The minimum value of the roll range for the affix</param>
	/// <param name="maxValue">The maximum value of the roll range for the affix</param>
	/// <typeparam name="T">The affix type to create.</typeparam>
	/// <returns>The new affix.</returns>
	public static Affix CreateAffix<T>(float minValue = 0f, float maxValue = 1f)
	{
		Affix instance = (Affix)Activator.CreateInstance(typeof(T)) ?? throw new Exception($"Could not create affix of type {typeof(T).Name}");
		instance.MinValue = minValue;
		instance.MaxValue = maxValue;
		instance.Roll();

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
			PoTMod.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
			return null;
		}

		var affix = (T)Activator.CreateInstance(t);

		affix.Load(tag);
		return affix;
	}

	/// <summary>
	/// Generates an affix from a binary reader, used on load to re-populate affixes
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static ItemAffix FromBReader(BinaryReader reader)
	{
		int aId = reader.ReadInt32();
		Type t = AffixHandler.ItemAffixTypeFromIndex(aId);
		if (t is null)
		{
			PoTMod.Instance.Logger.Error($"Could not load affix of internal id {aId}");
			return null;
		}

		var affix = (ItemAffix)Activator.CreateInstance(t);
		affix.NetReceive(reader);
		return affix;
	}

	/// <summary>
	/// Generates an affix from a binary reader, used on load to re-populate affixes
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	internal static MobAffix RecieveMobAffix(BinaryReader reader)
	{
		int aId = reader.ReadInt32();
		Type t = AffixHandler.MobAffixTypeFromIndex(aId);
		if (t is null)
		{
			PoTMod.Instance.Logger.Error($"Could not load affix of internal id {aId}");
			return null;
		}

		var affix = (MobAffix)Activator.CreateInstance(t);
		affix.NetReceive(reader);
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

	internal virtual void CreateLocalization()
	{
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
	public static List<ItemAffix> GetAffixes(Item item)
	{
		return _itemAffixes
			.Where(proto => proto.RequiredInfluence == Influence.None ||
			                proto.RequiredInfluence == item.GetInstanceData().Influence)
			.Where(proto => (item.GetInstanceData().ItemType & proto.PossibleTypes) == item.GetInstanceData().ItemType)
			.ToList();
	}

	public static List<ItemAffix> GetAffixes()
	{
		return _itemAffixes;
	}

	public static Type ItemAffixTypeFromIndex(int idx)
	{
		return _itemAffixes[idx].GetType();
	}

	public static int IndexFromItemAffix(Affix affix)
	{
		ItemAffix a = _itemAffixes.First(a => affix.GetType() == a.GetType());

		if (a is null)
		{
			return 0;
		}

		return _itemAffixes.IndexOf(a);
	}

	public static Type MobAffixTypeFromIndex(int idx)
	{
		return _mobAffixes[idx].GetType();
	}

	public static int IndexFromMobAffix(MobAffix affix)
	{
		MobAffix a = _mobAffixes.First(a => affix.GetType() == a.GetType());

		if (a is null)
		{
			return 0;
		}

		return _mobAffixes.IndexOf(a);
	}

	/// <summary>
	/// Returns a list of mob affixes that are valid for the given type. Typically used to roll affixes.
	/// </summary>
	/// <returns></returns>
	public static List<MobAffix> GetAffixes(ItemRarity rarity)
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

		foreach (Type type in AssemblyManager.GetLoadableTypes(mod.Code))
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Affix)))
			{
				continue;
			}

			var instance = Activator.CreateInstance(type) as Affix;
			instance.OnLoad();
			instance.CreateLocalization();

			switch (instance)
			{
				case ItemAffix itemAffix:
					_itemAffixes.Add(itemAffix);
					continue;
				case MobAffix mobAffix:
					_mobAffixes.Add(mobAffix);

					MobAffix.MobAffixIconsByAffixName[mobAffix.GetType().AssemblyQualifiedName] = ModContent.Request<Texture2D>(mobAffix.TexturePath);
					break;
			}
		}

		_itemAffixes.Sort((a1, a2) => a1.GetType().FullName.CompareTo(a2.GetType().FullName));
		_mobAffixes.Sort((a1, a2) => a1.GetType().FullName.CompareTo(a2.GetType().FullName));
	}

	public void Unload()
	{
		foreach (ItemAffix item in _itemAffixes)
		{
			item.OnUnload();
		}

		foreach (MobAffix item in _mobAffixes)
		{
			item.OnUnload();
		}

		_itemAffixes = null;
		_mobAffixes = null;
	}
}
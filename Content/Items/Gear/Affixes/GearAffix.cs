﻿using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Items.Gear.Affixes;

internal abstract class GearAffix : Affix
{
	public ModifierType ModifierType = ModifierType.Passive;

	public GearInfluence RequiredInfluence = GearInfluence.None;

	public GearType PossibleTypes = 0;

	public virtual void BuffPassive(Player player, Gear gear) { }

	public abstract string GetTooltip(Player player, Gear gear);
		
	public abstract float GetModifierValue(Gear gear);

	public GearAffix Clone()
	{
		var clone = (GearAffix)Activator.CreateInstance(GetType());

		clone._minValue = _minValue;
		clone._maxValue = _maxValue;
		clone.Value = Value;
		clone.RequiredInfluence = RequiredInfluence;
		clone.PossibleTypes = PossibleTypes;

		return clone;
	}

	/// <summary>
	/// Generates an affix from a tag, used on load to re-populate affixes
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static GearAffix FromTag(TagCompound tag)
	{
		var affix = (GearAffix)Activator.CreateInstance(typeof(GearAffix).Assembly.GetType(tag.GetString("type")));

		if (affix is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
			return null;
		}

		affix.Load(tag);
		return affix;
	}
}

internal class AffixHandler : ILoadable
{
	private static List<GearAffix> _prototypes = [];

	/// <summary>
	/// Returns a list of affixes that are valid for the given type. Typically used to roll affixes.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="influence"></param>
	/// <returns></returns>
	public static List<GearAffix> GetAffixes(GearType type, GearInfluence influence)
	{
		return _prototypes
			.Where(proto => proto.RequiredInfluence == GearInfluence.None || proto.RequiredInfluence == influence)
			.Where(proto => (type & proto.PossibleTypes) == type)
			.ToList();
	}

	public void Load(Mod mod)
	{
		_prototypes = [];

		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(GearAffix))) continue;
			object instance = Activator.CreateInstance(type);
			_prototypes.Add(instance as GearAffix);
		}
	}

	public void Unload()
	{
		_prototypes = null;
	}
}
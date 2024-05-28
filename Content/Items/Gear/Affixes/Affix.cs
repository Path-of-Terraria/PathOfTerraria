using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Chat.Commands;
using Terraria.ModLoader.IO;
using static System.Net.Mime.MediaTypeNames;

namespace PathOfTerraria.Content.Items.Gear.Affixes;

internal abstract class Affix
{
	private float _minValue;
	private float _maxValue = 1;
	private float _externalMultiplier = 1;
	protected float Value = 1;
	public virtual ModifierType ModifierType => ModifierType.Passive;
	public virtual bool IsFlat => true; // alternative is percent
	public virtual bool Round => false;
	public virtual GearInfluence RequiredInfluence => GearInfluence.None;

	public abstract GearType PossibleTypes { get; }

	public virtual void BuffPassive(Player player, Gear gear) { }

	public abstract string Tooltip { get; }
	public string GetTooltip(Player player, Gear gear)
	{
		float value = GetModifierValue(gear);
		bool positive = value >= 0;
		string text = MathF.Abs(value).ToString();

		string range = "";
		if (Main.keyState.PressingShift())
		{
			float oVal = Value;

			Value = _minValue;
			float valueMin = GetModifierValue(gear);
			bool positivMin = valueMin >= 0;
			string textMin = valueMin.ToString();

			Value = _maxValue;
			float valueMax = GetModifierValue(gear);
			bool positivMax = valueMax >= 0;
			string textMax = valueMax.ToString();

			Console.WriteLine("-----------");
			Console.WriteLine(valueMin + " | " + valueMax);
			Console.WriteLine(textMin + " | " + textMax);

			range = $" [{textMin} - {textMax}]";

			Value = oVal;
		}

		text = text + range;

		if (IsFlat && ModifierType == ModifierType.Multiplier)
		{
			text = (value * 100f).ToString("0.0") + range + "%";
		}

		if (!IsFlat)
		{
			text += "%";
		}

		Console.WriteLine(text);

		text = ModifierType switch
		{
			ModifierType.Added => positive ? "+" + text : "-" + text,
			ModifierType.Multiplier => text + (positive ? " increased" : " decreased"),
			_ => text
		};
		Console.WriteLine(text + " | " + Tooltip + " - " + Tooltip.Replace("#", text));

		return Tooltip.Replace("#", text);
	}

	protected abstract float internalModifierCalculation(Gear gear);
	public float GetModifierValue(Gear gear) {
		float v = internalModifierCalculation(gear) * _externalMultiplier;

		if (Round)
		{
			v = (float)Math.Round(v);
		}
		else
		{
			v = (float)Math.Round(v, 2);
		}
		
		return v;
	}
	public int GetModifierIValue(Gear gear) { return (int)GetModifierValue(gear); }

	public static Affix CreateAffix<T>(float externalMultiplier = 1, float value = -1)
	{
		Affix instance = (Affix)Activator.CreateInstance(typeof(T));

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

	public Affix Clone()
	{
		var clone = (Affix)Activator.CreateInstance(GetType());

		clone._minValue = _minValue;
		clone._maxValue = _maxValue;
		clone.Value = Value;

		return clone;
	}

	public virtual void Roll()
	{
		Value = Main.rand.Next((int)(_minValue * 10), (int)(_maxValue * 10)) / 10f;
	}

	public virtual void Save(TagCompound tag)
	{
		tag["type"] = GetType().FullName;
		tag["externalMultiplier"] = _externalMultiplier;
		tag["value"] = Value;
	}

	public virtual void Load(TagCompound tag)
	{
		_externalMultiplier = tag.GetFloat("externalMultiplier");
		Value = tag.GetFloat("value");
	}

	/// <summary>
	/// Generates an affix from a tag, used on load to re-populate affixes
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static Affix FromTag(TagCompound tag)
	{
		var affix = (Affix)Activator.CreateInstance(typeof(Affix).Assembly.GetType(tag.GetString("type")));

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
	private static List<Affix> _prototypes = new();

	/// <summary>
	/// Returns a list of affixes that are valid for the given type. Typically used to roll affixes.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="influence"></param>
	/// <returns></returns>
	public static List<Affix> GetAffixes(GearType type, GearInfluence influence)
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
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Affix))) continue;
			object instance = Activator.CreateInstance(type);
			_prototypes.Add(instance as Affix);
		}
	}

	public void Unload()
	{
		_prototypes = null;
	}
}
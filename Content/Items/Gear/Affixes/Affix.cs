using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace FunnyExperience.Content.Items.Gear.Affixes
{
	internal abstract class Affix
	{
		private float _minValue;
		private float _maxValue = 1;
		protected float Value = 1;

		public GearInfluence RequiredInfluence = GearInfluence.None;

		public GearType PossibleTypes = 0;

		public virtual void BuffPassive(Player player, Gear gear) { }

		public abstract string GetTooltip(Player player, Gear gear);

		public Affix Clone()
		{
			var clone = (Affix)Activator.CreateInstance(GetType());

			clone._minValue = _minValue;
			clone._maxValue = _maxValue;
			clone.Value = Value;
			clone.RequiredInfluence = RequiredInfluence;
			clone.PossibleTypes = PossibleTypes;

			return clone;
		}

		public virtual void Roll()
		{
			Value = Main.rand.Next((int)(_minValue * 10), (int)(_maxValue * 10)) / 10f;
		}

		public virtual void Save(TagCompound tag)
		{
			tag["type"] = GetType().FullName;
			tag["value"] = Value;
		}

		public virtual void Load(TagCompound tag)
		{
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
				FunnyExperience.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
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
				.Where(proto => (proto.PossibleTypes & type) > 0)
				.ToList();
		}

		public void Load(Mod mod)
		{
			_prototypes = new List<Affix>();

			foreach (Type type in FunnyExperience.Instance.Code.GetTypes())
			{
				if (!type.IsAbstract && type.IsSubclassOf(typeof(Affix)))
				{
					object instance = Activator.CreateInstance(type);
					_prototypes.Add(instance as Affix);
				}
			}
		}

		public void Unload()
		{
			_prototypes = null;
		}
	}
}
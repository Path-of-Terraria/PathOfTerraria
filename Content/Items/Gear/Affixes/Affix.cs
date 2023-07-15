using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace FunnyExperience.Content.Items.Gear.Affixes
{
	internal abstract class Affix
	{
		public float minValue = 0;
		public float maxValue = 1;
		public float value = 1;

		public GearInfluence requiredInfluence = GearInfluence.None;

		public GearType possibleTypes = 0;

		public virtual void BuffPassive(Player player, Gear gear) { }

		public abstract string GetTooltip(Player player, Gear gear);

		public Affix Clone()
		{
			var clone = (Affix)Activator.CreateInstance(GetType());

			clone.minValue = minValue;
			clone.maxValue = maxValue;
			clone.value = value;
			clone.requiredInfluence = requiredInfluence;
			clone.possibleTypes = possibleTypes;

			return clone;
		}

		public virtual void Roll()
		{
			value = Main.rand.Next((int)(minValue * 10), (int)(maxValue * 10)) / 10f;
		}

		public virtual void Save(TagCompound tag)
		{
			tag["type"] = GetType().FullName;
			tag["value"] = value;
		}

		public virtual void Load(TagCompound tag)
		{
			value = tag.GetFloat("value");
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
				FunnyExperience.instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
				return null;
			}

			affix.Load(tag);
			return affix;
		}
	}

	internal class AffixHandler : ILoadable
	{
		public static List<Affix> prototypes = new();

		/// <summary>
		/// Returns a list of affixes that are valid for the given type. Typically used to roll affixes.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static List<Affix> GetAffixes(GearType type, GearInfluence influence)
		{
			var affixes = new List<Affix>();

			foreach (Affix proto in prototypes)
			{
				if (proto.requiredInfluence != GearInfluence.None && proto.requiredInfluence != influence)
					continue;

				if ((proto.possibleTypes & type) > 0)
					affixes.Add(proto);
			}

			return affixes;
		}

		public void Load(Mod mod)
		{
			prototypes = new();

			foreach (Type type in FunnyExperience.instance.Code.GetTypes())
			{
				if (!type.IsAbstract && type.IsSubclassOf(typeof(Affix)))
				{
					object instance = Activator.CreateInstance(type);
					prototypes.Add(instance as Affix);
				}
			}
		}

		public void Unload()
		{
			prototypes = null;
		}
	}
}

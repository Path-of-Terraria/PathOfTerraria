using PathOfTerraria.Content.Items.Gear;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Affixes;

internal abstract class GearAffix : Affix
{
	// public virtual ModifierType ModifierType => ModifierType.Passive;
	// public virtual bool IsFlat => true; // alternative is percent
	// public virtual bool Round => false;
	public virtual GearInfluence RequiredInfluence => GearInfluence.None;
	public abstract GearType PossibleTypes { get; }

	public virtual void ApplyAffix(EntityModifier modifier, Gear gear) { }

	public string GetTooltip(Gear gear)
	{
		EntityModifier modifier = new();
		ApplyAffix(modifier, gear);

		string tooltip = "";

		List<string> affixes = EntityModifier.GetChange(modifier);

		if (affixes.Any())
		{
			tooltip = affixes.First(); // idk if there will ever be more..?
		}

		return tooltip;
	}
	
	/// <summary>
	/// Generates an affix from a tag, used on load to re-populate affixes
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static GearAffix FromTag(TagCompound tag)
	{
		Type t = typeof(GearAffix).Assembly.GetType(tag.GetString("type"));
		if (t is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
			return null;
		}
		
		var affix = (GearAffix)Activator.CreateInstance(t);

		if (affix is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
			return null;
		}

		affix.Load(tag);
		return affix;
	}
}
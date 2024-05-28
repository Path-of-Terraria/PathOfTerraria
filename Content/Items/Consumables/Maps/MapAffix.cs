using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public abstract class MapAffix : Affix
{
	public abstract string GetTooltip(Map map);
	
	/// <summary>
	/// Generates an affix from a tag, used on load to re-populate affixes
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static MapAffix FromTag(TagCompound tag)
	{
		var affix = (MapAffix)Activator.CreateInstance(typeof(MapAffix).Assembly.GetType(tag.GetString("type")));

		if (affix is null)
		{
			PathOfTerraria.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
			return null;
		}

		affix.Load(tag);
		return affix;
	}
}
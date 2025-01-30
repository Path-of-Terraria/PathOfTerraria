using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds;

internal class MappingNPC : GlobalNPC
{
	public override void SetDefaults(NPC entity)
	{
		if (SubworldSystem.Current is MappingWorld map && map.Affixes is not null)
		{
			foreach (MapAffix affix in map.Affixes)
			{
				affix.ModifyNewNPC(entity);
			}
		}
	}
}

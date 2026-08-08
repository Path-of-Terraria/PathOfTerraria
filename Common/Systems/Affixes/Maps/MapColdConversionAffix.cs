using PathOfTerraria.Common.Systems.ElementalDamage;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class MapColdConversionAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC ele))
		{
			ele.Container.AddElementalValues((ElementType.Cold, 0, Value / 100f));
		}
	}
}

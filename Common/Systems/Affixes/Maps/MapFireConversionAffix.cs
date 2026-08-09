using PathOfTerraria.Common.Systems.ElementalDamage;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class MapFireConversionAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC ele))
		{
			ele.Container.AddElementalValues((ElementType.Fire, 0, Value / 100f));
		}
	}
}

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class MapDamageAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		npc.damage = (int)(npc.damage * (1 + Value / 100f));
	}
}

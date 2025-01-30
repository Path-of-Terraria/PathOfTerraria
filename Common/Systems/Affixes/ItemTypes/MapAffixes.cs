using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

public abstract class MapAffix : ItemAffix
{
	public virtual void ModifyNewNPC(NPC npc)
	{
	}
}

public class MapDamageAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		npc.damage = (int)(npc.damage * (1 + Value / 100f));
	}
}

public class MapBossHealthAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.boss)
		{
			npc.lifeMax = (int)(npc.lifeMax * (1 + Value / 100f));
			npc.life = npc.lifeMax;
		}
	}
}

public class MapMobCritChanceAffix : MapAffix
{
	public class MobCritChance : GlobalNPC
	{
		public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
		{
			if (SubworldSystem.Current is MappingWorld world)
			{

			}
		}
	}
}
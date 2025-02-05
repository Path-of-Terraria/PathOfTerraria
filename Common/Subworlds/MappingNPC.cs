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

			if (map.AreaLevel > 50)
			{
				float modifier = 1 + (map.AreaLevel - 50) / 10f;
				entity.life = entity.lifeMax = (int)(entity.lifeMax * modifier);
				entity.defDamage = entity.damage = (int)(entity.damage * modifier);
			}
		}
	}

	public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
	{
		if (SubworldSystem.Current is MappingWorld map && map.Affixes is not null)
		{
			foreach (MapAffix affix in map.Affixes)
			{
				affix.ModifyHitPlayer(npc, target, ref modifiers);
			}
		}
	}

	public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
	{
		if (SubworldSystem.Current is MappingWorld map && map.Affixes is not null)
		{
			foreach (MapAffix affix in map.Affixes)
			{
				affix.OnHitPlayer(npc, target, hurtInfo);
			}
		}
	}

	public override bool PreAI(NPC npc)
	{
		if (SubworldSystem.Current is MappingWorld map && map.Affixes is not null)
		{
			foreach (MapAffix affix in map.Affixes)
			{
				affix.PreAI(npc);
			}
		}

		return true;
	}
}

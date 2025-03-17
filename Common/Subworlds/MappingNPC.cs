using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
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

	public override void OnKill(NPC npc)
	{
		if (npc.boss && SubworldSystem.Current is MappingWorld world)
		{
			MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;

			if (DownedBossForTier(world))
			{
				tracker.AddCompletion(world.MapTier);
			}

			int count = tracker.CompletionsAtOrAboveTier(0);

			if (count >= 10 && !NPC.downedQueenSlime)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<QueenSlimeMap>());
			}
			
			count = tracker.CompletionsAtOrAboveTier(1);

			if (count >= 10 && NPC.downedQueenSlime && !NPC.downedMechBoss2)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<TwinsMap>());
			}

			count = tracker.CompletionsAtOrAboveTier(3);

			if (count >= 10 && NPC.downedQueenSlime && !NPC.downedMechBoss2)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<PrimeMap>());
			}
		}
	}

	private static bool DownedBossForTier(MappingWorld world)
	{
		return world.MapTier switch
		{
			1 => NPC.downedQueenSlime,
			2 => NPC.downedMechBoss1,
			3 => NPC.downedMechBoss2,
			4 => NPC.downedMechBoss3,
			_ => true,
		};
	}
}

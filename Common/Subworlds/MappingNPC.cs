using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

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
		if (npc.boss && SubworldSystem.Current is MappingWorld world && Main.hardMode && PoTItemHelper.PickItemLevel() >= 45)
		{
			MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;

			if (DownedBossForTier(world))
			{
				tracker.AddCompletion(world.MapTier);
			}

			Dictionary<int, int> completionsByTier = tracker.CompletionsPerTier();

			if (TierPassed(0) && !NPC.downedQueenSlime)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<QueenSlimeMap>());
			}
			
			if (TierPassed(1) && NPC.downedQueenSlime && !NPC.downedMechBoss2)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<TwinsMap>());
			}
			
			if (TierPassed(2) && NPC.downedMechBoss2 && !NPC.downedMechBoss1)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<DestroyerMap>());
			}

			if (TierPassed(3) && NPC.downedMechBoss1 && !NPC.downedMechBoss3)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<PrimeMap>());
			}

			if (TierPassed(4) && NPC.downedMechBoss3 && !NPC.downedPlantBoss)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<PlanteraMap>());
			}

			if (TierPassed(5) && NPC.downedPlantBoss && !NPC.downedGolemBoss)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<GolemMap>());
			}

			if (TierPassed(6) && NPC.downedGolemBoss && !NPC.downedFishron)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<FishronMap>());
			}

			if (TierPassed(7) && NPC.downedGolemBoss && !NPC.downedEmpressOfLight)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<EoLMap>());
			}

			if (TierPassed(8) && NPC.downedGolemBoss && !NPC.downedEmpressOfLight)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, ModContent.ItemType<CultistMap>());
			}

			return;

			bool TierPassed(int tier)
			{
				return completionsByTier.TryGetValue(tier, out int tierValue) && tierValue >= 10;
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
			5 => NPC.downedPlantBoss,
			6 => NPC.downedGolemBoss,
			7 => NPC.downedFishron,
			8 => NPC.downedEmpressOfLight,
			9 => NPC.downedAncientCultist,
			_ => true,
		};
	}
}

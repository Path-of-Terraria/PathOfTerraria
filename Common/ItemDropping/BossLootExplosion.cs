using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.ItemDropping;

/// <summary>
/// Bursts a currency-weighted shower of items when a boss is killed inside a boss domain or
/// exploration map. Boss kills are otherwise excluded from <see cref="ArpgNPC.OnKill"/>'s
/// regular mob drop pipeline, so this hook gives the climax of a boss fight a real ARPG payoff.
/// </summary>
internal sealed class BossLootExplosion : GlobalNPC
{
	/// <summary> Currency-favored burst weights (vs. the default 80/15/5). </summary>
	private static readonly DropTable.DropCategoryWeights BurstWeights = new(Gear: 0.35f, Currency: 0.60f, Map: 0.05f);

	/// <summary> Flat rarity boost applied to every rolled drop in the burst. </summary>
	public const float BaseRarityBoost = 0.5f;

	public override void OnKill(NPC npc)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || !ShouldBurst(npc)) //Stops if not boss
		{
			return;
		}

		int areaLevel = PoTMobHelper.GetAreaLevel();
		int count = ComputeBurstCount(areaLevel);
		float rarityModifier = BaseRarityBoost + ArpgNPC.DomainRarityBoost();

		List<ItemDatabase.ItemRecord> drops;

		using (SmartLoot.Begin())
		{
			drops = DropTable.RollManyMobDrops(
				count,
				areaLevel,
				rarityModifier,
				BurstWeights,
				applyAreaLevelCategoryScaling: false);
		}

		for (int i = 0; i < drops.Count; i++)
		{
			ItemDatabase.ItemRecord record = drops[i];

			if (record == ItemDatabase.InvalidItem)
			{
				continue;
			}

			SpawnBurstItem(npc, record, areaLevel, i, drops.Count);
		}
	}

	private static void SpawnBurstItem(NPC npc, ItemDatabase.ItemRecord record, int areaLevel, int index, int total)
	{
		var item = new Item(record.ItemId);

		if (item.TryGetGlobalItem<PoTInstanceItemData>(out _))
		{
			PoTInstanceItemData data = item.GetInstanceData();
			PoTStaticItemData staticData = item.GetStaticData();

			data.Rarity = staticData.IsUnique ? ItemRarity.Unique : record.Rarity;

			using (SmartLoot.Begin())
			{
				PoTItemHelper.Roll(item, areaLevel);
			}
		}

		int spawned = Item.NewItem(npc.GetSource_Death(), npc.Center, Vector2.Zero, item);

		if (spawned < 0 || spawned >= Main.maxItems)
		{
			return;
		}

		// Scatter outward in a starburst with a strong upward component so the drops feel like an explosion.
		float angle = MathHelper.TwoPi * (index / (float)Math.Max(total, 1));
		Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
		velocity.Y -= Main.rand.NextFloat(2f, 5f);
		Main.item[spawned].velocity = velocity;

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.SyncItem, -1, -1, null, spawned);
		}
	}

	private static bool ShouldBurst(NPC npc)
	{
		bool isBoss = npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];

		if (!isBoss)
		{
			return false;
		}

		if (!ShouldCountBossKill(npc))
		{
			return false;
		}

		// Lunar pillars aren't the climax — Cultist and Moon Lord are.
		if (npc.type is NPCID.LunarTowerNebula or NPCID.LunarTowerSolar or NPCID.LunarTowerStardust or NPCID.LunarTowerVortex)
		{
			return false;
		}

		// Boss domains and exploration maps only — skip Ravencrest hub and the overworld.
		return SubworldSystem.Current is BossDomainSubworld or MappingWorld;
	}

	private static bool ShouldCountBossKill(NPC npc)
	{
		return npc.type switch
		{
			NPCID.Retinazer => !NPC.AnyNPCs(NPCID.Spazmatism),
			NPCID.Spazmatism => !NPC.AnyNPCs(NPCID.Retinazer),
			NPCID.GolemFistLeft or NPCID.GolemFistRight or NPCID.GolemHead or NPCID.GolemHeadFree => false,
			_ => true,
		};
	}

	private static int ComputeBurstCount(int areaLevel)
	{
		return 5 + (int)(areaLevel * 0.3f);
	}
}

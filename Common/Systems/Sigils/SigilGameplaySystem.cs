using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Conflux;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Content.Buffs.ShrineBuffs;
using PathOfTerraria.Content.Conflux;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Mapping.Sigils;
using PathOfTerraria.Content.NPCs.Mapping.Desert;
using PathOfTerraria.Content.NPCs.Mapping.Forest;
using PathOfTerraria.Content.NPCs.Mapping.Sigils;
using PathOfTerraria.Content.Swamp;
using PathOfTerraria.Content.Swamp.NPCs;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Utilities.Terraria;
using SubworldLibraryCommunityFork;
using Terraria.DataStructures;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Sigils;

internal static class SigilSpawnContext
{
	private static (int BonusAffixes, float LifeMultiplier, float DamageMultiplier)? pending;

	public static void Prepare(int bonusAffixes, float lifeMultiplier = 1f, float damageMultiplier = 1f)
	{
		pending = (Math.Max(0, bonusAffixes), Math.Max(0.1f, lifeMultiplier), Math.Max(0.1f, damageMultiplier));
	}

	public static bool TryConsume(out int bonusAffixes, out float lifeMultiplier, out float damageMultiplier)
	{
		if (pending is not { } value)
		{
			bonusAffixes = 0;
			lifeMultiplier = damageMultiplier = 1f;
			return false;
		}

		pending = null;
		(bonusAffixes, lifeMultiplier, damageMultiplier) = value;
		return true;
	}

	public static void Clear()
	{
		pending = null;
	}
}

internal sealed class SigilEncounterNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public int EncounterOwner = -1;
	public bool Nemesis;
	public bool ConfluxEmpowered;
	public byte TyrantReinforcementStage;

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		bitWriter.WriteBit(Nemesis);
		bitWriter.WriteBit(ConfluxEmpowered);
		binaryWriter.Write((short)EncounterOwner);
		binaryWriter.Write(TyrantReinforcementStage);
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		Nemesis = bitReader.ReadBit();
		ConfluxEmpowered = bitReader.ReadBit();
		EncounterOwner = binaryReader.ReadInt16();
		TyrantReinforcementStage = binaryReader.ReadByte();
	}

	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || !SigilSystem.IsExplorationMap()
			|| npc.friendly || npc.boss || npc.lifeMax <= 5)
		{
			return;
		}

		if (SigilSystem.FindFamily(SigilFamily.Conflux) is not { } conflux)
		{
			return;
		}

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.ModProjectile is ConfluxRift && projectile.DistanceSQ(npc.Center) < 1800f * 1800f)
			{
				float life = conflux.Kind == SigilKind.TriuneConflux ? 1.30f : conflux.Grade switch
				{
					SigilGrade.Gilded => 1.20f,
					SigilGrade.Prismatic => 1.30f,
					_ => 1f,
				};
				float damage = conflux.Kind == SigilKind.TriuneConflux ? 1.20f : 1f;
				npc.lifeMax = Math.Max(1, (int)(npc.lifeMax * life));
				npc.life = npc.lifeMax;
				npc.damage = Math.Max(1, (int)(npc.damage * damage));
				ConfluxEmpowered = true;
				break;
			}
		}
	}

	public override void SetDefaults(NPC npc)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || !SigilSystem.IsExplorationMap()
			|| !npc.boss || SigilSystem.FindFamily(SigilFamily.Sovereignty) is not { } sigil)
		{
			return;
		}

		(float life, float damage) = sigil.Kind == SigilKind.CrownedTyrant
			? (2.50f, 1.50f)
			: sigil.Grade switch
			{
				SigilGrade.Carved => (1.25f, 1.10f),
				SigilGrade.Gilded => (1.50f, 1.20f),
				_ => (2.00f, 1.35f),
			};

		npc.lifeMax = Math.Max(1, (int)(npc.lifeMax * life));
		npc.life = npc.lifeMax;
		npc.damage = Math.Max(1, (int)(npc.damage * damage));
	}

	public override void PostAI(NPC npc)
	{
		if (!npc.boss || !SigilSystem.Has(SigilKind.CrownedTyrant) || Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		float lifeRatio = npc.life / (float)Math.Max(npc.lifeMax, 1);
		if (TyrantReinforcementStage == 0 && lifeRatio <= 0.66f)
		{
			TyrantReinforcementStage = 1;
			npc.netUpdate = true;
			SigilEncounterSystem.SpawnElitePack(npc.Center, 3, 3, npc.whoAmI);
		}
		else if (TyrantReinforcementStage == 1 && lifeRatio <= 0.33f)
		{
			TyrantReinforcementStage = 2;
			npc.netUpdate = true;
			SigilEncounterSystem.SpawnElitePack(npc.Center, 4, 3, npc.whoAmI);
		}
	}

	public override void OnKill(NPC npc)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || !SigilSystem.IsExplorationMap())
		{
			return;
		}

		if (Nemesis)
		{
			SigilEncounterSystem.MarkNemesisCompleted();
			SigilRewards.SpawnLootRoll(npc, new DropTable.DropCategoryWeights(0.80f, 0.15f, 0.05f));
		}

		bool isMapBoss = npc.boss && BossLootExplosion.ShouldCountBossKill(npc);
		if (isMapBoss)
		{
			int previousTier = SigilSystem.HighestCompletedMapTier;
			int previousSlots = SigilSystem.UnlockedSlotCount;
			int completedTier = Math.Max(MappingWorld.MapTier, Map.TierBasedOnWorldLevel(MappingWorld.AreaLevel));
			SigilSystem.RecordMapCompletion(completedTier);
			if (previousTier == 0)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Center, ModContent.ItemType<CarvedSigilOfBinding>());
			}
			if (SigilSystem.UnlockedSlotCount > previousSlots)
			{
				AnnounceSlotUnlock(SigilSystem.UnlockedSlotCount);
			}
			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.WorldData);
			}

			SigilRewards.TryDropCartographyMaps(npc);
			if (SigilSystem.Has(SigilKind.CrownedTyrant))
			{
				SigilGrade grade = Main.rand.NextBool() ? SigilGrade.Gilded : SigilGrade.Prismatic;
				int guaranteed = SigilCatalog.RollNormalType(grade);
				if (guaranteed > 0) { Item.NewItem(npc.GetSource_Death(), npc.Center, guaranteed); }
			}
		}

		float chance = isMapBoss ? 0.30f : npc.GetGlobalNPC<ArpgNPC>().Rarity switch
		{
			ItemRarity.Rare => 0.04f,
			ItemRarity.Magic => 0.01f,
			_ => 0.0015f,
		};

		if (Main.rand.NextFloat() < chance)
		{
			int itemType = SigilCatalog.RollDropType(MappingWorld.MapTier, allowUnique: isMapBoss);
			if (itemType > 0)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Center, itemType);
			}
		}
	}

	private static void AnnounceSlotUnlock(int slots)
	{
		string key = $"Mods.{PoTMod.ModName}.Misc.Sigils.SlotUnlocked";
		Color color = new(221, 174, 76);
		if (Main.netMode == NetmodeID.Server)
		{
			ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key, slots), color);
		}
		else
		{
			Main.NewText(Language.GetTextValue(key, slots), color);
		}
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (!SigilSystem.IsExplorationMap() || SigilSystem.FindFamily(SigilFamily.Infestation) is not { } sigil || spawnRate == int.MinValue)
		{
			return;
		}

		float morePacks = sigil.Grade switch
		{
			SigilGrade.Carved => 0.15f,
			SigilGrade.Gilded => 0.30f,
			_ => 0.50f,
		};
		spawnRate = Math.Max(1, (int)(spawnRate / (1f + morePacks)));
		maxSpawns += Math.Max(1, (int)MathF.Ceiling(8f * morePacks));
	}
}

internal sealed class SigilEncounterSystem : ModSystem
{
	private static bool initialized;
	private static int spawnRetryTimer;
	private static int completedNemeses;
	private static int completedCaches;
	private static int completedShrines;

	internal static int RequestedNemeses { get; private set; }
	internal static int RequestedCaches { get; private set; }
	internal static int RequestedShrines { get; private set; }
	internal static int SpawnedNemeses => Math.Min(RequestedNemeses, completedNemeses + CountLivingNemeses());
	internal static int SpawnedCaches => Math.Min(RequestedCaches, completedCaches + CountLivingCaches());
	internal static int SpawnedShrines => Math.Min(RequestedShrines, completedShrines + CountLivingShrines());

	public override void ClearWorld()
	{
		initialized = false;
		spawnRetryTimer = 0;
		RequestedNemeses = RequestedCaches = RequestedShrines = 0;
		completedNemeses = completedCaches = completedShrines = 0;
		SigilSpawnContext.Clear();
	}

	public override void SaveWorldData(TagCompound tag)
	{
		if (!SigilSystem.IsExplorationMap())
		{
			return;
		}

		tag["sigilCompletedNemeses"] = completedNemeses;
		tag["sigilCompletedCaches"] = completedCaches;
		tag["sigilCompletedShrines"] = completedShrines;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		completedNemeses = Math.Max(0, tag.GetInt(tag.ContainsKey("sigilCompletedNemeses") ? "sigilCompletedNemeses" : "scarabCompletedNemeses"));
		completedCaches = Math.Max(0, tag.GetInt(tag.ContainsKey("sigilCompletedCaches") ? "sigilCompletedCaches" : "scarabCompletedCaches"));
		completedShrines = Math.Max(0, tag.GetInt(tag.ContainsKey("sigilCompletedShrines") ? "sigilCompletedShrines" : "scarabCompletedShrines"));
	}

	public override void PostUpdateWorld()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || !SigilSystem.IsExplorationMap() || !HasActivePlayer())
		{
			return;
		}

		if (!initialized)
		{
			RequestedNemeses = SigilSystem.Power(SigilFamily.Nemeses) is int nemesisPower && nemesisPower > 0 ? nemesisPower + 1 : 0;
			RequestedCaches = SigilSystem.Power(SigilFamily.WardedWealth);
			RequestedShrines = SigilSystem.Power(SigilFamily.Devotion);
			initialized = true;
		}

		if (spawnRetryTimer-- > 0)
		{
			return;
		}

		bool spawned = false;
		if (completedNemeses + CountLivingNemeses() < RequestedNemeses)
		{
			spawned = TrySpawnNemesis();
		}
		else if (completedCaches + CountLivingCaches() < RequestedCaches)
		{
			spawned = TrySpawnStationary(ModContent.NPCType<WardedCacheNPC>(), SigilSystem.Power(SigilFamily.WardedWealth));
		}
		else if (completedShrines + CountLivingShrines() < RequestedShrines)
		{
			spawned = TrySpawnStationary(ModContent.NPCType<ConsecratedShrineNPC>(), SigilSystem.Power(SigilFamily.Devotion));
		}

		spawnRetryTimer = spawned ? 15 : 90;
	}

	private static bool HasActivePlayer()
	{
		foreach (Player player in Main.ActivePlayers)
		{
			if (!player.dead) { return true; }
		}
		return false;
	}

	private static bool TrySpawnNemesis()
	{
		if (!TryFindMapPosition(out Vector2 position))
		{
			return false;
		}

		int power = SigilSystem.Power(SigilFamily.Nemeses);
		SigilSpawnContext.Prepare(power, 1f + power * 0.20f, 1f + power * 0.08f);
		NPC npc;
		try
		{
			npc = NPC.NewNPCDirect(new EntitySource_Misc("SigilNemesis"), position, ChooseMapEnemy());
		}
		finally
		{
			SigilSpawnContext.Clear();
		}

		if (!npc.active)
		{
			return false;
		}

		npc.GetGlobalNPC<SigilEncounterNPC>().Nemesis = true;
		npc.GetGlobalNPC<NPCDespawning>().NeverDespawn = true;
		npc.netUpdate = true;
		return true;
	}

	private static bool TrySpawnStationary(int type, int power)
	{
		if (!TryFindMapPosition(out Vector2 position))
		{
			return false;
		}

		NPC npc = NPC.NewNPCDirect(new EntitySource_Misc("SigilMapContent"), position, type, ai0: power);
		npc.netUpdate = true;
		return npc.active;
	}

	internal static void SpawnElitePack(Vector2 center, int count, int bonusAffixes, int owner)
	{
		for (int i = 0; i < count; i++)
		{
			SigilSpawnContext.Prepare(bonusAffixes, 1.25f, 1.10f);
			NPC npc;
			try
			{
				npc = NPC.NewNPCDirect(new EntitySource_Misc("SigilEncounterWave"), center + Main.rand.NextVector2Circular(220f, 80f), ChooseMapEnemy());
			}
			finally
			{
				SigilSpawnContext.Clear();
			}

			if (npc.active)
			{
				npc.GetGlobalNPC<SigilEncounterNPC>().EncounterOwner = owner;
				npc.GetGlobalNPC<NPCDespawning>().NeverDespawn = true;
				npc.netUpdate = true;
			}
		}
	}

	internal static bool HasLivingChildren(int owner)
	{
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.GetGlobalNPC<SigilEncounterNPC>().EncounterOwner == owner) { return true; }
		}
		return false;
	}

	internal static void MarkNemesisCompleted() => completedNemeses++;
	internal static void MarkCacheCompleted() => completedCaches++;
	internal static void MarkShrineCompleted() => completedShrines++;

	private static int CountLivingNemeses()
	{
		int count = 0;
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.GetGlobalNPC<SigilEncounterNPC>().Nemesis) { count++; }
		}
		return count;
	}

	private static int CountLivingCaches()
	{
		int count = 0;
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.ModNPC is WardedCacheNPC) { count++; }
		}
		return count;
	}

	private static int CountLivingShrines()
	{
		int count = 0;
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.ModNPC is ConsecratedShrineNPC) { count++; }
		}
		return count;
	}

	private static bool TryFindMapPosition(out Vector2 position)
	{
		var placement = new SpawnPlacement
		{
			Area = new Rectangle(40, 40, Main.maxTilesX - 80, Main.maxTilesY - 80),
			CollisionSize = new Point(10 * 16, 8 * 16),
			OnGround = true,
			MinDistanceFromPlayers = 700f,
			MaxDistanceFromPlayers = 0f,
			MinDistanceFromEnemies = 900f,
			MaxDistanceFromEnemies = 0f,
			MaxSearchAttempts = 4096,
			SkippedLiquids = LiquidMask.All,
		};
		return EnemySpawning.TryFindingSpawnPosition(out position, in placement);
	}

	internal static int ChooseMapEnemy()
	{
		if (SubworldSystem.Current is ForestArea)
		{
			int[] pool = [ModContent.NPCType<Entling>(), ModContent.NPCType<EntlingAlt>(), ModContent.NPCType<ClumsyEntling>(), NPCID.Wraith, NPCID.PossessedArmor];
			return Main.rand.Next(pool);
		}

		if (SubworldSystem.Current is DesertArea)
		{
			int[] pool = [ModContent.NPCType<HauntedHead>(), NPCID.Mummy, NPCID.DesertGhoul, NPCID.DesertDjinn];
			return Main.rand.Next(pool);
		}

		int[] swampPool = [ModContent.NPCType<DragonFly>(), ModContent.NPCType<Mudsquit>(), ModContent.NPCType<SwampCroc>()];
		return Main.rand.Next(swampPool);
	}
}

internal static class SigilRewards
{
	public static void SpawnLootRoll(NPC source, DropTable.DropCategoryWeights weights)
	{
		ItemDatabase.ItemRecord record = DropTable.RollMobDrops(PoTItemHelper.PickItemLevel(), ArpgNPC.DomainRarityBoost(), weights);
		if (record == ItemDatabase.InvalidItem)
		{
			return;
		}

		Item item = MapChestLoot.BuildChestItem(record);
		Item.NewItem(source.GetSource_Death(), source.Center, item);
	}

	public static void TryDropCartographyMaps(NPC boss)
	{
		if (SigilSystem.FindFamily(SigilFamily.Cartography) is not { } sigil)
		{
			return;
		}

		int rolls = sigil.Grade switch
		{
			SigilGrade.Carved => Main.rand.NextBool(2) ? 1 : 0,
			SigilGrade.Gilded => 1,
			_ => 1 + (Main.rand.NextBool(2) ? 1 : 0),
		};
		for (int i = 0; i < rolls; i++)
		{
			ItemDatabase.ItemRecord record = DropTable.RollMobDrops(PoTItemHelper.PickItemLevel(), 0f, new DropTable.DropCategoryWeights(0f, 0f, 1f), applyAreaLevelCategoryScaling: false);
			if (record == ItemDatabase.InvalidItem)
			{
				continue;
			}

			var item = new Item(record.ItemId);
			if (item.ModItem is Map map)
			{
				int upgradeChance = sigil.Grade switch
				{
					SigilGrade.Carved => 15,
					SigilGrade.Gilded => 30,
					_ => 50,
				};
				map.Tier = Math.Clamp(MappingWorld.MapTier + (Main.rand.Next(100) < upgradeChance ? 1 : 0), 1, Map.MaxMapTier);
				PoTItemHelper.Roll(item, Map.WorldLevelBasedOnTier(map.Tier));
			}
			Item.NewItem(boss.GetSource_Death(), boss.Center, item);
		}
	}
}

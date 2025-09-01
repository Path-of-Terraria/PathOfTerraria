using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Systems.ModPlayers;
using SubworldLibrary;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using System.Linq;

namespace PathOfTerraria.Core.Items;

/// <summary>
///		Utilities for tasks related to interacting with items and our item data
///		added by Path of Terraria.
/// </summary>
public static class PoTItemHelper
{
	#region Data retrieval

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PoTInstanceItemData GetInstanceData(this Item item)
	{
		return item.GetGlobalItem<PoTInstanceItemData>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PoTInstanceItemData GetInstanceData(this ModItem item)
	{
		return item.Item.GetInstanceData();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PoTStaticItemData GetStaticData(this Item item)
	{
		return PoTGlobalItem.GetStaticData(item.type);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PoTStaticItemData GetStaticData(this ModItem item)
	{
		return item.Item.GetStaticData();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GearInstanceData GetGearData(this Item item)
	{
		return item.GetGlobalItem<GearInstanceData>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GearInstanceData GetGearData(this ModItem item)
	{
		return item.Item.GetGearData();
	}

	#endregion

	#region Rolling

	public static void Roll(Item item, int itemLevel)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		SetItemLevel.Invoke(item, itemLevel);

		RollAffixes(item);
		PostRoll.Invoke(item);
		data.SpecialName = GenerateName.Invoke(item);
	}

	private static void RollAffixes(Item item)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		data.Affixes.Clear();
		data.Affixes.AddRange(GenerateImplicits.Invoke(item));
		data.ImplicitCount = data.Affixes.Count;

		int affixesToRoll = GetAffixCount(item);
		for (int i = 0; i < affixesToRoll; i++)
		{
			AddNewAffix(item, data);
		}

		List<ItemAffix> uniqueItemAffixes = GenerateAffixes.Invoke(item);

		foreach (ItemAffix affix in uniqueItemAffixes)
		{
			affix.Roll();
		}

		data.Affixes.AddRange(uniqueItemAffixes);
	}
	
	/// <summary>
	/// Re-rolls the values of all affixes on an item. Keeping the tiers and affixes the same.
	/// </summary>
	/// <param name="item"></param>
	public static void RerollAffixValues(Item item)
	{
	    PoTInstanceItemData data = item.GetInstanceData();
	    foreach (ItemAffix affix in data.Affixes)
	    {
	        affix.Value = AffixRegistry.GetRandomAffixValue(affix, GetItemLevel.Invoke(item));
	    }
	}

	///  <summary> 
	///  Adds a new random affix to an item. This is used for things like the ascendant shard. 
	///  </summary>
	///  <param name="item"></param>
	///  <param name="data"></param>
	public static void AddNewAffix(Item item, [CanBeNull] PoTInstanceItemData data = null)
	{
		data ??= item.GetInstanceData();
		if ((data.Affixes.Count - data.ImplicitCount) >= GetAffixCount(item))
		{
			return;
		}

		ItemAffixData chosenAffix = AffixRegistry.GetRandomAffixDataByItemType(data.ItemType, excludedAffixes: data.Affixes.Select(a => a.GetData()));
		if (chosenAffix is null)
		{
			return;
		}

		ItemAffix affix = AffixRegistry.ConvertToItemAffix(chosenAffix);
		if (affix is null)
		{
			return;
		}

		affix.Value = AffixRegistry.GetRandomAffixValue(affix, GetItemLevel.Invoke(item));
		if (affix.Value == 0)
		{
			return; //If the affix has no value, don't add it. This usually happens when there's no TierData associated with the given item
		}

		data.Affixes.Add(affix);
	}

	#endregion

	#region Affixes

	public static void ApplyAffixes(Item item, EntityModifier entityModifier, Player player)
	{
		foreach (ItemAffix affix in item.GetInstanceData().Affixes)
		{
			affix.ApplyAffix(player, entityModifier, item);
			player?.GetModPlayer<AffixPlayer>().AddStrength(affix.GetType().AssemblyQualifiedName, affix.Value);
		}
	}

	public static void ClearAffixes(Item item)
	{
		item.GetInstanceData().Affixes.Clear();
	}

	public static int GetAffixCount(Item item)
	{
		return GetAffixCount(item.GetInstanceData().Rarity);
	}

	public static int GetAffixCount(ItemRarity rarity)
	{
		return rarity switch
		{
			ItemRarity.Magic => Main.rand.Next(1, GetMaxAffixCounts(rarity) + 1),
			ItemRarity.Rare => Main.rand.Next(3, GetMaxAffixCounts(rarity) + 1),
			_ => 0
		};
	}

	public static int GetMaxAffixCounts(ItemRarity rarity)
	{
		return rarity switch
		{
			ItemRarity.Magic => 2,
			ItemRarity.Rare => 4,
			_ => 0
		};
	}

	/// <summary>
	/// Gets the max amount of affixes a mob can have based on the rarity.
	/// </summary>
	/// <param name="rarity">Rarity of the mob.</param>
	/// <returns>How many affixes the mob can have.</returns>
	public static int GetMaxMobAffixCounts(ItemRarity rarity)
	{
		return rarity switch
		{
			ItemRarity.Magic => 2,
			ItemRarity.Rare => 4,
			_ => 0
		};
	}

	public static bool HasMaxAffixesForRarity(Item item)
	{
		PoTInstanceItemData data = item.GetInstanceData();
		int nonImplicitAffixCount = data.Affixes.Count(affix => !affix.IsImplicit);
		return nonImplicitAffixCount >= GetMaxAffixCounts(data.Rarity);
	}

	public static void SetMouseItemToHeldItem(Player player)
	{
		if (player.selectedItem == 58)
		{
			Main.mouseItem = player.HeldItem;
		}
	}

	#endregion

	/// <summary>
	/// Picks the most appropriate item level for the current world. This is the following:<br/>
	/// For explorable maps, such as the Forest, it's variable and depends on the tier of the map.<br/>
	/// Boss domains/overworld progression uses an additive system where each boss adds 5 levels:<br/>
	/// Base level: 5<br/>
	/// King Slime: +5 (total: 10)<br/>
	/// Eye of Cthulhu: +5 (total: 15)<br/>
	/// Eater of Worlds: +5 (total: 20)<br/>
	/// Brain of Cthulhu: +5 (total: 25, or 30 if both corruption bosses killed)<br/>
	/// Queen Bee: +5<br/>
	/// Deerclops: +5<br/>
	/// Skeletron: +5<br/>
	/// <b>Hardmode minimum/crafting cap: 45</b><br/>
	/// Queen Slime: +5 (total: 50)<br/>
	/// Twins: +5 (total: 55)<br/>
	/// Destroyer: +5 (total: 60)<br/>
	/// Skeletron Prime: + (total: 65)br/>
	/// Plantera: +5 (total: 70)<br/>
	/// Golem: +5 (total: 75)<br/>
	/// Cultist: +5 (total: 80)<br/>
	/// Moon Lord: +5 (maximum: 85)
	/// </summary>
	/// <param name="clampHardmode">Clamps level to hardmode maximum (45) for crafted items.</param>
	/// <param name="isCrafted">Indicates if the item is being crafted, which applies hardmode capping.</param>
	/// <returns>The calculated item level based on world progression.</returns>

	public static int PickItemLevel(bool clampHardmode = true, bool isCrafted = false)
	{
		if (SubworldSystem.Current is MappingWorld && MappingWorld.AreaLevel > 0 && !isCrafted)
		{
			return MappingWorld.AreaLevel;
		}

		if (isCrafted && Main.hardMode) // This accounts for crafting level when you've progressed further than WoF
		{
			return 45;
		}

		if (clampHardmode && Main.hardMode) // Hardmode max if it's clamped.
		{
			return 45;
		}

		// Start with base level and add for each boss defeated
		int level = 5;

		if (NPC.downedSlimeKing)
		{
			level += 5; // 10
		}

		if (NPC.downedBoss1)
		{
			level += 5; // 15
		}

		if (EventTracker.HasFlagsAnywhere(EventFlags.DefeatedEaterOfWorlds))
		{
			level += 5; // 20
		}

		if (EventTracker.HasFlagsAnywhere(EventFlags.DefeatedBrainOfCthulhu))
		{
			level += 5; // 20/25
		}

		if (NPC.downedQueenBee)
		{
			level += 5; // 25/30
		}

		if (NPC.downedDeerclops)
		{
			level += 5; // 30/35
		}

		if (NPC.downedBoss3)
		{
			level += 5; // 35/40
		}

		if (Main.hardMode)
		{
			level = 45;
		}

		// Continue with hardmode bosses if not clamped
		if (!clampHardmode)
		{
			if (NPC.downedQueenSlime)
			{
				level += 5; // 50
			}

			if (NPC.downedMechBoss2)
			{
				level += 5; // 55
			}

			if (NPC.downedMechBoss1)
			{
				level += 5; // 60
			}

			if (NPC.downedMechBoss3) 
			{
				level += 5; // 65
			}

			if (NPC.downedPlantBoss)
			{
				level += 5; // 70
			}

			if (NPC.downedGolemBoss)
			{
				level += 5; // 75
			}

			if (NPC.downedAncientCultist)
			{
				level += 5; // 80
			}

			if (NPC.downedMoonlord)
			{
				level += 5; // 85
			}
		}

		return level;

	}
}
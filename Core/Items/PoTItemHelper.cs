using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Utilities;
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
		data.NameAffix = GenerateNameAffixes.Invoke(item);
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
	        affix.Value = AffixRegistry.GetRandomAffixValue(affix, item, GetItemLevel.Invoke(item));
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
		if ((data.Affixes.Count - data.ImplicitCount) >= GetMaxAffixCounts(data.Rarity))
		{
			return;
		}

		IEnumerable<ItemAffixData> exclusions = data.Affixes.SelectExcept(null, a => a.TryGetData(item));
		ItemAffixData chosenAffix = AffixRegistry.GetRandomAffixData(item, excludedAffixes: exclusions);

		if (chosenAffix is null)
		{
			return;
		}

		ItemAffix affix = AffixRegistry.ConvertToItemAffix(chosenAffix);
		if (affix is null)
		{
			return;
		}

		affix.Value = AffixRegistry.GetRandomAffixValue(affix, item, GetItemLevel.Invoke(item));
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
	/// Picks the most appropriate item level for the current world. The overworld scales up to 70
	/// based on boss progression; explorable maps continue past that with +1 per tier.<br/>
	/// World start: 1<br/>
	/// King Slime: 5<br/>
	/// Eye of Cthulhu: 10<br/>
	/// Evil Boss (Eater of Worlds OR Brain of Cthulhu): 15<br/>
	/// Queen Bee: 20<br/>
	/// Deerclops: 25<br/>
	/// Skeletron: 30<br/>
	/// Wall of Flesh: 35<br/>
	/// Queen Slime: 40<br/>
	/// Twins: 45<br/>
	/// Destroyer: 50<br/>
	/// Skeletron Prime: 55<br/>
	/// Plantera: 60<br/>
	/// Golem: 65<br/>
	/// Cultist: 70<br/>
	/// Moon Lord: 70 (cap)
	/// </summary>
	/// <param name="clampHardmode">Unused; retained for API compatibility. The overworld now scales to 70 naturally.</param>
	/// <param name="isCrafted">If true, ignores the current <see cref="MappingWorld.AreaLevel"/> override and uses overworld progression.</param>
	/// <returns>The calculated item level based on world progression.</returns>
	public static int PickItemLevel(bool clampHardmode = true, bool isCrafted = false)
	{
		if (SubworldSystem.Current is MappingWorld && MappingWorld.AreaLevel > 0 && !isCrafted)
		{
			return MappingWorld.AreaLevel;
		}

		int level = 1;

		if (NPC.downedSlimeKing)
		{
			level = 5;
		}

		if (NPC.downedBoss1)
		{
			level = 10;
		}

		// Evil Boss: either Eater of Worlds OR Brain of Cthulhu satisfies the milestone.
		if (EventTracker.HasFlagsAnywhere(EventFlags.DefeatedEaterOfWorlds) || EventTracker.HasFlagsAnywhere(EventFlags.DefeatedBrainOfCthulhu))
		{
			level = 15;
		}

		if (NPC.downedQueenBee)
		{
			level = 20;
		}

		if (NPC.downedDeerclops)
		{
			level = 25;
		}

		if (NPC.downedBoss3)
		{
			level = 30;
		}

		if (Main.hardMode)
		{
			level = 35;
		}

		if (NPC.downedQueenSlime)
		{
			level = 40;
		}

		if (NPC.downedMechBoss2)
		{
			level = 45;
		}

		if (NPC.downedMechBoss1)
		{
			level = 50;
		}

		if (NPC.downedMechBoss3)
		{
			level = 55;
		}

		if (NPC.downedPlantBoss)
		{
			level = 60;
		}

		if (NPC.downedGolemBoss)
		{
			level = 65;
		}

		if (NPC.downedAncientCultist || NPC.downedMoonlord)
		{
			level = 70;
		}

		return level;
	}
}

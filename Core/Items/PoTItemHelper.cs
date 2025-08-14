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
	/// 		Adds a new random affix to an item. This is used for things like the ascendant shard.
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

		ItemAffixData chosenAffix = AffixRegistry.GetRandomAffixDataByItemType(data.ItemType);
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
		data.Affixes.Add(affix);
	}

	#endregion

	#region Affixes

	public static void ApplyAffixes(Item item, EntityModifier entityModifier, Player player)
	{
		foreach (ItemAffix affix in item.GetInstanceData().Affixes)
		{
			affix.ApplyAffix(player, entityModifier, item);
			affix.ApplyTooltip(player, item, player.GetModPlayer<UniversalBuffingPlayer>().AffixTooltipHandler);
			player?.GetModPlayer<AffixPlayer>().AddStrength(affix.GetType().AssemblyQualifiedName, affix.Value);
		}
	}

	public static void ApplyAffixTooltips(Item item, Player player)
	{
		foreach (ItemAffix affix in item.GetInstanceData().Affixes)
		{
			affix.ApplyTooltip(player, item, player.GetModPlayer<UniversalBuffingPlayer>().AffixTooltipHandler);
			//player?.GetModPlayer<AffixPlayer>().AddStrength(affix.GetType().AssemblyQualifiedName, affix.Value);
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
		return data.Affixes.Count >= GetMaxAffixCounts(data.Rarity);
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
	/// Boss domains/overworld progression, in order:<br/>
	/// Default: 5<br/>
	/// King Slime: 10<br/>
	/// Eye of Cthulhu: 15<br/>
	/// Eater of Worlds: 20<br/>
	/// Brain of Cthulhu: 25<br/>
	/// Queen Bee: 30<br/>
	/// Deerclops: 35<br/>
	/// Skeletron: 40<br/>
	/// Wall of Flesh / <b>Overworld Max</b>: 45<br/>
	/// Queen Slime: 50<br/>
	/// Twins: 55<br/>
	/// Destroyer: 60<br/>
	/// Skeletron Prime: 65<br/>
	/// Plantera: 70<br/>
	/// Golem: 75<br/>
	/// Cultist: 80<br/>
	/// Moon Lord: 85
	/// </summary>
	/// <param name="clampHardmode">Clamps level to hardmode (45).</param>
	/// <returns></returns>
	public static int PickItemLevel(bool clampHardmode = true)
	{
		if (SubworldSystem.Current is MappingWorld mapWorld && mapWorld.AreaLevel > 0)
		{
			return mapWorld.AreaLevel;
		}

		if (clampHardmode && Main.hardMode)
		{
			return 45;
		}

		if (NPC.downedMoonlord)
		{
			return 85;
		}

		if (NPC.downedAncientCultist)
		{
			return 80;
		}

		if (NPC.downedGolemBoss)
		{
			return 75;
		}

		if (NPC.downedPlantBoss)
		{
			return 70;
		}

		if (NPC.downedMechBoss3) // Skele Prime
		{
			return 65;
		}

		if (NPC.downedMechBoss1) // Destroyer
		{
			return 60;
		}

		if (NPC.downedMechBoss2) // Twins
		{
			return 55;
		}

		if (NPC.downedQueenSlime)
		{
			return 50;
		}

		if (Main.hardMode)
		{
			return 45;
		}

		if (NPC.downedBoss3) //Skeletron
		{
			return 40;
		}

		if (NPC.downedDeerclops)
		{
			return 35;
		}
		
		if (NPC.downedQueenBee)
		{
			return 30;
		}

		if (BossTracker.DownedBrainOfCthulhu)
		{
			return 25;
		}

		if (BossTracker.DownedEaterOfWorlds)
		{
			return 20;
		}

		if (NPC.downedBoss1) //Eye of Cthulhu
		{
			return 15;
		}

		if (NPC.downedSlimeKing)
		{
			return 10;
		}

		return 5;
	}
}
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Gear.Armor.Leggings;

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
		PoTStaticItemData staticData = item.GetStaticData();

		SetItemLevel.Invoke(item, itemLevel);

		// Only level 50+ gear can get influence.
		if (data.RealLevel > 50 && !staticData.IsUnique && (data.ItemType & ItemType.AllGear) == ItemType.AllGear)
		{
			// Quality does not affect influence right now.
			// Might not need to, seems to generaet plenty often late game.
			int inf = Main.rand.Next(400) - data.RealLevel;

			if (inf < 30)
			{
				data.Influence = Main.rand.NextBool() ? Influence.Solar : Influence.Lunar;
			}
		}

		RollAffixes(item);
		PostRoll.Invoke(item);
		data.SpecialName = GenerateName.Invoke(item);
	}

	public static void Reroll(Item item)
	{
		// TODO: Don't call ANY variant of SetDefaults here... please?
		item.GetInstanceData().Affixes.Clear();
		item.ModItem?.SetDefaults();
		Roll(item, PickItemLevel());
	}

	private static void RollAffixes(Item item)
	{
		PoTInstanceItemData data = item.GetInstanceData();
		PoTStaticItemData staticData = item.GetStaticData();

		data.Affixes.Clear();
		data.Affixes.AddRange(GenerateAffixes.Invoke(item));
		data.ImplicitCount = data.Affixes.Count;

		for (int i = 0; i < GetAffixCount(item); i++)
		{
			AddNewAffix(item, data);
		}

		List<ItemAffix> uniqueItemAffixes = GenerateImplicits.Invoke(item);

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
		if (data.Affixes.Count >= GetAffixCount(item))
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

	// TODO: Un-hardcode?
	public static int PickItemLevel()
	{
		if (NPC.downedMoonlord)
		{
			return Main.rand.Next(150, 201);
		}

		if (NPC.downedAncientCultist)
		{
			return Main.rand.Next(110, 151);
		}

		if (NPC.downedGolemBoss)
		{
			return Main.rand.Next(95, 131);
		}

		if (NPC.downedPlantBoss)
		{
			return Main.rand.Next(80, 121);
		}

		if (NPC.downedMechBossAny)
		{
			return Main.rand.Next(75, 111);
		}

		if (Main.hardMode)
		{
			return 50;
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
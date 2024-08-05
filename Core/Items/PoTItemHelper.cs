using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Data;

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
			ItemAffixData chosenAffix = AffixRegistry.GetRandomAffixDataByItemType(data.ItemType);
			if (chosenAffix is null)
			{
				continue;
			}

			ItemAffix affix = AffixRegistry.ConvertToItemAffix(chosenAffix);
			if (affix is null)
			{
				continue;
			}

			affix.Value = AffixRegistry.GetRandomAffixValue(affix, GetItemLevel.Invoke(item));
			data.Affixes.Add(affix);
		}

		if (staticData.IsUnique)
		{
			List<ItemAffix> uniqueItemAffixes = GenerateAffixes.Invoke(item);

			foreach (ItemAffix affix in uniqueItemAffixes)
			{
				affix.Roll();
			}

			data.Affixes.AddRange(uniqueItemAffixes);
		}
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
		return item.GetInstanceData().Rarity switch
		{
			ItemRarity.Magic => 2,
			ItemRarity.Rare => Main.rand.Next(3, 5),
			_ => 0
		};
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
			return Main.rand.Next(50, 91);
		}

		if (NPC.downedBoss3)
		{
			return Main.rand.Next(30, 50);
		}

		if (NPC.downedBoss2)
		{
			return Main.rand.Next(20, 41);
		}

		if (NPC.downedBoss1)
		{
			return Main.rand.Next(10, 26);
		}

		return Main.rand.Next(5, 21);
	}
}
